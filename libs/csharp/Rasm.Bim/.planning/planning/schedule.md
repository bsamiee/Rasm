# [BIM_SCHEDULE]

The host-neutral 4D construction-sequencing and CPM DOMAIN owner: one `ScheduleNetwork` record carrying the activity network — the `ConstructionTask` row whose `IfcTaskTime` schedule and actual start/finish fold onto NodaTime `Interval`s over the work-calendar zone, the closed `SequenceRel` `[Union]` (`FinishToStart`/`StartToStart`/`FinishToFinish`) carrying each `IfcRelSequence` dependency lag as a NodaTime `Period`, the `TaskAssignment` projecting `IfcRelAssignsToProcess` to join a task to the `Model/query#ELEMENT_SET`-selected `BimElement` set, and the `ScheduleProjection.Project` fold that folds the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` container into the typed network — plus the `WorkCalendar` working-time fold over `IfcWorkCalendar`/`IfcWorkTime`/`IfcRecurrencePattern`, the `CriticalPath` forward/backward-pass fold over the `SequenceRel` DAG, and the `ConstructionState.At(Instant)` snapshot that reads the element set whose task `Interval.Contains` the queried instant. The schedule is a VIEW of the federated `Model/elements#ELEMENT_MODEL` `BimModel`, never a re-modeled element graph: a task carries its assigned-element GlobalIds, the snapshot resolves them against the one `BimModel.Elements` index, and the `csharp:Rasm.AppUi/schedule` schedule report and the `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence consume the typed network by reference. The schedule is HOST-NEUTRAL — the calendar values ride NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalTime`/`IsoDayOfWeek` over the model's work-calendar zone and never a BCL `DateTime` on a public signature — and the task-to-element join is the `Model/query#ELEMENT_SET` `ElementPredicate` algebra selecting the assigned set, never a second selection surface. The activity network is the `Planning/cost#ESTIMATE` 5D pairing's time axis: a `ConstructionResource` joins `sequencing#ConstructionTask` by its `GlobalId` and the `CostSchedule.Rollup` reads the task `Interval` the schedule owns, the `CriticalPath` float window feeds the `Planning/cost#ESTIMATE` `EarnedValue` schedule-performance fold, so this page authors the `[2]-[SCHEDULE]` `ConstructionTask` anchor the cost resource-join cluster cites by reference rather than re-deriving the activity network. This is the host-neutral CPM/4D DOMAIN owner the `csharp:Rasm.Persistence/Sync/schedule # [WIRE]` seam relocation settles: `Rasm.Bim/schedule` owns the `CriticalPath`/`WorkCalendar`/`ConstructionState.At` fold over the IFC-projected `SequenceRel` network, while Persistence keeps the durable Primavera P6 XER and MS-Project XML store and its own `CpmPass` over the external `TaskRelation` DAG — the two read disjoint source lanes and federate by the IFC `GlobalId` (the Persistence `ScheduleTask.GlobalId` correlation joins each external activity to this owner's `ConstructionTask.GlobalId`, never the external P6 `TaskId`), and a Bim `CriticalPath` reading a P6 relation or a Persistence `CpmPass` re-deriving the IFC `SequenceRel` network is the cross-package drift defect the relocation makes explicit. A schedule rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family — a task naming an assigned-element GlobalId the federated model never declares is `BimFault.DanglingReference`, a dependency edge naming an absent process or a cyclic dependency the forward pass cannot topologically order is `BimFault.ModelRejected`, never a sixth arm.

## [01]-[INDEX]

- [01]-[SCHEDULE]: `ScheduleNetwork` record, the `ConstructionTask` record (`IfcTaskTime` as a NodaTime `Interval`), the `SequenceRel` `[Union]` (`FinishToStart`/`StartToStart`/`FinishToFinish`) with `Period` lag, the `TaskAssignment` record, the `TaskStatus`/`WorkScheduleKind` `[SmartEnum]` vocabularies, the `ScheduleProjection.Project` fold from the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` surface, and the `ConstructionState.At(Instant)` element-set snapshot.
- [02]-[CRITICAL_PATH]: the `WorkCalendar` working-time fold over `IfcWorkCalendar`/`IfcWorkTime`/`IfcRecurrencePattern`, the `CriticalPath` value record per task, and the `ScheduleNetwork.Schedule` forward/backward-pass CPM fold over the `SequenceRel` DAG by topological order.

## [02]-[SCHEDULE]

- Owner: `ScheduleNetwork` the single host-neutral 4D activity-network record carrying the task set, the dependency-edge set, the assignment set, the work-calendar zone the calendar values resolve against, and the `(GeometryKey, ScheduleKey)` content-key identity the AppUi report and Persistence federation read it by; `ConstructionTask` the task row promoting one `IfcTask`/`IfcTaskTime` into a first-class owner carrying its stable `GlobalId`, name, `TaskStatus`, milestone flag, the scheduled `Interval` and the optional actual `Interval` (both over the model's work-calendar zone), and the `WorkScheduleKind` the parent `IfcWorkSchedule`/`IfcWorkPlan` predefines; `SequenceRel` the closed `[Union]` discriminating the three IFC dependency modalities — `FinishToStart` (the predecessor's finish gates the successor's start), `StartToStart` (the two starts are gated together), `FinishToFinish` (the two finishes are gated together) — each carrying its predecessor/successor task GlobalIds and the dependency `Period` lag the `IfcRelSequence.TimeLag` `IfcLagTime` declares; `TaskAssignment` the `IfcRelAssignsToProcess` row joining one task to the `Model/query#ELEMENT_SET`-selected assigned-`BimElement` GlobalId set; `TaskStatus` the `[SmartEnum<string>]` over the IFC task status; `WorkScheduleKind` the `[SmartEnum<string>]` over `IfcWorkScheduleTypeEnum`; `ConstructionState` the snapshot owner folding the network at one `Instant`; `ScheduleProjection` the static fold over the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule` container.
- Cases: `SequenceRel` arms `FinishToStart` (`PredecessorGlobalId`, `SuccessorGlobalId`, `Period Lag`) · `StartToStart` (`PredecessorGlobalId`, `SuccessorGlobalId`, `Period Lag`) · `FinishToFinish` (`PredecessorGlobalId`, `SuccessorGlobalId`, `Period Lag`) (3) — the IFC `START_FINISH` member and the `USERDEFINED`/`NOTDEFINED` fallbacks fold onto `FinishToStart` through the `SequenceRel.Of` resolver so a guessed fourth modality never mints a parallel edge type; the `ConstructionTask` row carries its `GlobalId`, `Name`, `TaskStatus`, `bool IsMilestone`, the `Interval Scheduled` schedule window, the `Option<Interval> Actual` actual window (present once the task carries an `ActualStart`/`ActualFinish`), and the `WorkScheduleKind` — a milestone is a zero-duration `Interval` whose start equals its finish, while a spanning activity carries a positive-duration `Interval`; the `TaskAssignment` row carries its `TaskGlobalId` and the `Seq<string>` of assigned-element GlobalIds the `IfcRelAssignsToProcess.RelatedObjects` `IfcProduct` set names.
- Entry: `ScheduleProjection.Project(IfcWorkPlan plan, BimModel federated, DateTimeZone zone)` folds one GeometryGym work-plan container into one `ScheduleNetwork` — materializing the plan's controlled `IfcWorkSchedule` set once, folding each schedule's controlled `IfcTask` set onto `ConstructionTask` rows (reading the `TaskTime` `IfcTaskTime` schedule/actual start-finish onto the `Interval`s over `zone` and the `Status`/`IsMilestone`/`PredefinedType` onto the typed members), folding each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges (discriminating `SequenceType` onto the union arm and reading the `TimeLag` `IfcLagTime` onto the `Period`), folding each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows binding the assigned-`IfcProduct` GlobalIds against the `federated` element index, and deriving the `(GeometryKey, ScheduleKey)` identity — and `ScheduleProjection.ProjectAll(Seq<IfcWorkPlan> plans, BimModel federated, DateTimeZone zone)` lifts every work plan in a federated model onto the `Seq<ScheduleNetwork>` the report reads; `Fin<T>` aborts on a dependency edge naming a process the schedule never declares (`Model/faults#FAULT_BAND` `BimFault.ModelRejected`) or a task assigning a product GlobalId the federated model never declares (`BimFault.DanglingReference`) lowered with `.ToError()`. `ConstructionState.At(ScheduleNetwork network, BimModel federated, Instant instant)` reads the element set whose task `Interval.Contains(instant)` — folding the network's tasks to the active set (a task is active when its scheduled `Interval` contains the instant, preferring the actual `Interval` when present), unioning each active task's `TaskAssignment` element GlobalIds, and resolving them against the federated model through the `Model/query#ELEMENT_SET` `ElementSet.Where` GlobalId-membership combinator into one `ElementSet`, so the 4D playback at instant `t` is one fold over the network, never an enumerated per-task arm.
- Auto: `Project` reads the `IfcWorkPlan` runtime graph and folds it into the typed network — `SchedulesOf` materializes the plan's `Controls` `IfcRelAssignsToControl.RelatedObjects` `IfcWorkSchedule` set once and `TasksOf` materializes each schedule's controlled `IfcTask` set, the `TaskOf` projection reads `IfcTask.TaskTime` onto the scheduled `Interval` through `IntervalOf(taskTime.ScheduleStart, taskTime.ScheduleFinish, zone)` and the optional actual `Interval` through `ActualOf(taskTime.ActualStart, taskTime.ActualFinish, zone)` (each BCL `DateTime` lifting to a NodaTime `LocalDateTime` via `LocalDateTime.FromDateTime`, mapping into `zone` leniently through `InZoneLeniently` to absorb the daylight-transition gap/overlap an IFC calendar value carries no offset for, and projecting to the `Instant` the `Interval` bounds), reads `IfcTask.Status` onto the `TaskStatus` and `IfcTask.IsMilestone` onto the milestone flag and threads the controlling `IfcWorkSchedule.PredefinedType` onto the `WorkScheduleKind` from the `TasksOf` traversal that already owns the parent-schedule edge (the schedule's `Controls` `IfcRelAssignsToControl.RelatedObjects` `IfcTask` set), so the kind reads the owning schedule rather than re-resolving a task inverse; `SequencesOf` folds each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges discriminating `SequenceType` through `SequenceRel.Of` and reading the `TimeLag` `IfcLagTime.LagValue` `IfcDuration` onto the `Period` through `PeriodOf` (parsing the ISO-8601 duration the `IfcLagTime` declares onto a NodaTime `Period`); `AssignmentsOf` folds each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows reading the `RelatedObjects` `IfcProduct` GlobalIds; the `ScheduleNetwork.BindAssignments` fold resolves each assignment's element GlobalIds against the `Model/elements#ELEMENT_MODEL` `BimModel` element index so a task assigning a product the federated model never declares aborts the projection, and the `ScheduleNetwork.Identity` fold derives the `(GeometryKey, ScheduleKey)` `UInt128` pair the AppUi report and Persistence federation read the network by — `GeometryKey` over the assigned-element GlobalIds through `XxHash128.HashToUInt128` so the report re-reads only a network whose assigned geometry changed, and `ScheduleKey` over the task `Interval` bounds and the dependency-edge lags so a re-sequenced plan re-renders. `ConstructionState.At` folds the network at the instant: the `Active` fold filters the task set to those whose effective `Interval` (the `Actual` window when present, else the `Scheduled` window) contains the instant through `Interval.Contains`, the `Assigned` fold unions the active tasks' `TaskAssignment` element GlobalIds into one GlobalId set, and the `Model/query#ELEMENT_SET` `ElementSet.Where` combinator over the GlobalId-membership predicate resolves the active-element set against the federated model — so a Gantt scrub at instant `t` materializes the in-progress element set as a `Model/query#ELEMENT_SET` value the AppUi viewport renders.
- Receipt: the `Seq<ScheduleNetwork>` is the 4D activity-network evidence the `csharp:Rasm.AppUi/schedule` schedule report reads by the `(GeometryKey, ScheduleKey)` reference and the `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence stores by GlobalId, the `ConstructionState.At(Instant)` snapshot is the `Model/query#ELEMENT_SET` `ElementSet` the AppUi 4D viewport renders at a Gantt instant, and the `ConstructionTask` row is the `Planning/cost#ESTIMATE` 5D cost pairing's time axis the `ConstructionResource` joins by `GlobalId`; a scheduled milestone task, a finish-to-start dependency lag, and an assigned-element set each carry their typed calendar value on one network record.
- Packages: GeometryGymIFC_Core, NodaTime, Thinktecture.Runtime.Extensions, System.IO.Hashing, LanguageExt.Core, Rasm
- Growth: a new dependency modality is one `SequenceRel` union arm reading the next `IfcSequenceEnum` member; a new task status is one `TaskStatus` row; a new work-schedule kind is one `WorkScheduleKind` row reading the next `IfcWorkScheduleTypeEnum` member; a new calendar binding (a `IfcWorkCalendar` working-time exception) is one column on `ScheduleNetwork` read by the same fold; a new work plan rides the existing `ProjectAll` fold on one row; never a per-task-status record, never a second schedule store, never a `Get<Status>` task family, and never a re-modeled element graph on the schedule.
- Boundary: `ScheduleNetwork` is ONE record discriminated by the `SequenceRel` union over the dependency edges — a `FinishToStartRel`/`StartToStartRel`/`FinishToFinishRel` class family or three sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#ELEMENT_MODEL`; the `ConstructionTask` carries its calendar value as a NodaTime `Interval` over the model's work-calendar `DateTimeZone` and a BCL `DateTime`/`DateTimeOffset` field on the task or a public projection signature is the named host-neutrality defect — the IFC `DateTime` crosses the projection boundary once at `IntervalOf` and never reaches a domain signature, exploiting the admitted-but-unexploited NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalDateTime`/`Interval.Contains` surface (today only `Instant` is read) so the first calendar-arithmetic consumer beyond `Instant` recovers the under-utilized calendar primitives rather than re-deriving date math; the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask`/`IfcTaskTime`/`IfcRelSequence`/`IfcLagTime`/`IfcRelAssignsToProcess` surface (`.api/api-geometrygym-ifc` scheduling-cost-resource rows 1-8 + scheduling-traversal entrypoints 4-6) is consumed as settled vocabulary through the `IfcProcess`/`IfcTask` discrimination and a hand-rolled task reader is the deleted form; the task-to-element join is the `Model/query#ELEMENT_SET` `ElementPredicate` algebra resolving the assigned GlobalId set and a parallel schedule-element selection arm is the no-second-selection-surface reject — the schedule produces the assigned GlobalIds, the query owns the resolution; the `(GeometryKey, ScheduleKey)` content-key identity is derived through the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` idiom and the AppUi report and Persistence federation read the network by that reference — minting a second identity scheme for the report join is the named cross-folder drift defect; the `Planning/cost#ESTIMATE` 5D resource-join cluster cites this `[2]-[SCHEDULE]` `ConstructionTask` anchor by reference and re-deriving the activity network on the cost page is the deleted form; a schedule rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()` and a bare `Fin.Fail` without that lowering is the named seam defect.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.IO.Hashing;
using System.Text;
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Text;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class TaskStatus {
    public static readonly TaskStatus NotYetStarted = new("NOTYETSTARTED");
    public static readonly TaskStatus Started       = new("STARTED");
    public static readonly TaskStatus Completed      = new("COMPLETED");
    public static readonly TaskStatus Delayed        = new("DELAYED");
    public static readonly TaskStatus NotDefined     = new("NOTDEFINED");

    public static TaskStatus Of(string? status) =>
        TryGet((status ?? "").Trim().ToUpperInvariant()).IfNone(NotDefined);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
public sealed partial class WorkScheduleKind {
    public static readonly WorkScheduleKind Actual   = new("ACTUAL");
    public static readonly WorkScheduleKind Baseline = new("BASELINE");
    public static readonly WorkScheduleKind Planned  = new("PLANNED");
    public static readonly WorkScheduleKind NotDefined = new("NOTDEFINED");

    public static WorkScheduleKind Of(IfcWorkScheduleTypeEnum kind) =>
        TryGet(kind.ToString()).IfNone(NotDefined);
}

[Union]
public partial record SequenceRel {
    partial record FinishToStart(string PredecessorGlobalId, string SuccessorGlobalId, Period Lag);
    partial record StartToStart(string PredecessorGlobalId, string SuccessorGlobalId, Period Lag);
    partial record FinishToFinish(string PredecessorGlobalId, string SuccessorGlobalId, Period Lag);

    public string PredecessorGlobalId => Switch(
        finishToStart:  static r => r.PredecessorGlobalId,
        startToStart:   static r => r.PredecessorGlobalId,
        finishToFinish: static r => r.PredecessorGlobalId);

    public string SuccessorGlobalId => Switch(
        finishToStart:  static r => r.SuccessorGlobalId,
        startToStart:   static r => r.SuccessorGlobalId,
        finishToFinish: static r => r.SuccessorGlobalId);

    public Period Lag => Switch(
        finishToStart:  static r => r.Lag,
        startToStart:   static r => r.Lag,
        finishToFinish: static r => r.Lag);

    public static SequenceRel Of(IfcSequenceEnum kind, string predecessor, string successor, Period lag) =>
        kind switch {
            IfcSequenceEnum.START_START   => new StartToStart(predecessor, successor, lag),
            IfcSequenceEnum.FINISH_FINISH => new FinishToFinish(predecessor, successor, lag),
            _                             => new FinishToStart(predecessor, successor, lag),
        };
}

public sealed record ConstructionTask(
    string GlobalId,
    string Name,
    TaskStatus Status,
    WorkScheduleKind ScheduleKind,
    bool IsMilestone,
    Interval Scheduled,
    Option<Interval> Actual) {
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
            string.Join("", Assignments.Bind(static a => a.ElementGlobalIds).OrderBy(static id => id)))),
        XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(
            string.Join("", Tasks.Map(static t => $"{t.GlobalId}={t.Effective.Start.ToUnixTimeTicks()}-{t.Effective.End.ToUnixTimeTicks()}"))
                .Concat(string.Join("", Dependencies.Map(static d =>
                    $"{d.PredecessorGlobalId}>{d.SuccessorGlobalId}:{PeriodPattern.Roundtrip.Format(d.Lag)}"))))));

    public Fin<ScheduleNetwork> BindAssignments(BimModel federated) {
        var index = toHashSet(federated.Elements.Map(static e => e.GlobalId));
        return Assignments
            .Bind(static a => a.ElementGlobalIds)
            .Find(id => !index.Contains(id))
            .Match(
                Some: id => FinFail<ScheduleNetwork>(new BimFault.DanglingReference($"task-assigns-absent-element:{id}").ToError()),
                None: () => FinSucc(this));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ScheduleProjection {
    public static Fin<ScheduleNetwork> Project(IfcWorkPlan plan, BimModel federated, DateTimeZone zone) {
        var tasks = TasksOf(plan);
        return tasks
            .TraverseM(entry => TaskOf(entry.Task, entry.Kind, zone))
            .As()
            .Map(rows => new ScheduleNetwork(
                plan.GlobalId,
                plan.Name ?? "",
                zone,
                rows,
                SequencesOf(tasks.Map(static entry => entry.Task)),
                AssignmentsOf(tasks.Map(static entry => entry.Task))))
            .Bind(network => network.BindAssignments(federated));
    }

    public static Fin<Seq<ScheduleNetwork>> ProjectAll(Seq<IfcWorkPlan> plans, BimModel federated, DateTimeZone zone) =>
        plans.TraverseM(plan => Project(plan, federated, zone)).As();

    static Seq<IfcWorkSchedule> SchedulesOf(IfcWorkPlan plan) =>
        plan.Controls
            .AsIterable()
            .SelectMany(static rel => rel.RelatedObjects.AsIterable())
            .OfType<IfcWorkSchedule>()
            .ToSeq();

    static Seq<(IfcTask Task, WorkScheduleKind Kind)> TasksOf(IfcWorkPlan plan) =>
        SchedulesOf(plan)
            .AsIterable()
            .SelectMany(static schedule => schedule.Controls
                .AsIterable()
                .SelectMany(static rel => rel.RelatedObjects.AsIterable())
                .OfType<IfcTask>()
                .Select(task => (Task: task, Kind: WorkScheduleKind.Of(schedule.PredefinedType))))
            .ToSeq();

    static Fin<ConstructionTask> TaskOf(IfcTask task, WorkScheduleKind kind, DateTimeZone zone) =>
        IntervalOf(task.TaskTime?.ScheduleStart, task.TaskTime?.ScheduleFinish, zone)
            .Map(scheduled => new ConstructionTask(
                task.GlobalId,
                task.Name ?? "",
                TaskStatus.Of(task.Status),
                kind,
                task.IsMilestone,
                scheduled,
                ActualOf(task.TaskTime?.ActualStart, task.TaskTime?.ActualFinish, zone)));

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

    static Fin<Interval> IntervalOf(DateTime? start, DateTime? finish, DateTimeZone zone) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => FinSucc(new Interval(from, to)),
            ({ } from, { } to)                 => FinFail<Interval>(new BimFault.ModelRejected($"task-finish-before-start:{to}<{from}").ToError()),
            ({ } from, null)                   => FinSucc(new Interval(from, from)),
            _                                  => FinFail<Interval>(new BimFault.ModelRejected("task-missing-schedule-start").ToError()),
        };

    static Option<Interval> ActualOf(DateTime? start, DateTime? finish, DateTimeZone zone) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => Some(new Interval(from, to)),
            ({ } from, _)                      => Some(new Interval(from, from)),
            _                                  => None,
        };

    static Instant? InstantOf(DateTime? value, DateTimeZone zone) =>
        value is { } moment
            ? LocalDateTime.FromDateTime(moment).InZoneLeniently(zone).ToInstant()
            : null;

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
    public static ElementSet At(ScheduleNetwork network, BimModel federated, Instant instant) {
        var active = network.Tasks.Filter(task => task.ActiveAt(instant)).Map(static task => task.GlobalId);
        var assigned = toHashSet(network.Assignments
            .Filter(assignment => active.Contains(assignment.TaskGlobalId))
            .Bind(static assignment => assignment.ElementGlobalIds));
        return new ElementSet(federated.Elements).Where(element => assigned.Contains(element.GlobalId));
    }
}
```

## [03]-[CRITICAL_PATH]

- Owner: `WorkCalendar` the host-neutral working-time function folded from one `IfcWorkCalendar` — a work-week `IsoDayOfWeek` set, a daily shift `LocalTime` span, and the `ExceptionDay` `LocalDate` set the holiday/weather `IfcWorkTime`+`IfcRecurrencePattern` exception times expand — that maps a working-day count onto a calendar finish past the non-working days and an elapsed `Interval` onto its working `Duration`; `CriticalPath` the per-task value record carrying `EarlyStart`/`EarlyFinish`/`LateStart`/`LateFinish` as `Instant`, `TotalFloat`/`FreeFloat` as `Duration`, and the `bool IsCritical` zero-total-float flag; `ScheduleNetwork.Schedule` the forward/backward-pass CPM fold over the `SequenceRel` adjacency by topological order producing the `Map<string, CriticalPath>` per-task float window — one immutable fold over the edge set the network already owns, never a mutable PERT accumulator.
- Entry: `WorkCalendar.Of(IfcWorkCalendar calendar, DateTimeZone zone)` folds one GeometryGym work-calendar container into the typed working-time function — materializing the `WorkingTimes` `IfcWorkTime` set onto the work-week `IsoDayOfWeek` pattern and the daily shift `LocalTime` span (reading each `IfcWorkTime.RecurrencePattern` `IfcRecurrencePattern.WeekdayComponent` ISO-weekday set), and the `ExceptionTimes` `IfcWorkTime` set onto the `ExceptionDay` `LocalDate` set (expanding each exception's `IfcRecurrencePattern` `RecurrenceType`/`DayComponent`/`Interval`/`Occurrences` over the `StartDate`/`FinishDate` window through NodaTime `LocalDate` arithmetic) — the GeometryGym `RecurrencePattern` accessor being schema-internal, the fold reads the recurrence through the `IfcWorkTime` STEP graph the projection already holds rather than a re-resolved inverse; `WorkCalendar.Default` is the standard 5-day Monday-through-Friday 08:00–16:00 calendar a plan without an `IfcWorkCalendar` resolves against, so a duration-driven task always has a working-time function. `ScheduleNetwork.Schedule(WorkCalendar calendar)` folds the network into the `Map<string, CriticalPath>` — `Topological.Order` runs Kahn's algorithm over the `SequenceRel` predecessor→successor adjacency (a residual cycle aborting `BimFault.ModelRejected`), the forward pass folds each task in topological order deriving `EarlyStart` as the maximum over predecessors of the predecessor's modality-shifted finish plus the edge `Period` lag (a `FinishToStart` shifts the predecessor `EarlyFinish`, a `StartToStart` the predecessor `EarlyStart`, a `FinishToFinish` the predecessor `EarlyFinish` minus the task duration) and `EarlyFinish` through `calendar.Advance(EarlyStart, taskDuration)`, the backward pass folds in reverse topological order deriving `LateFinish` as the minimum over successors and `LateStart` through `calendar.Recede`, and `TotalFloat = LateStart − EarlyStart`, `FreeFloat = min successor EarlyStart − EarlyFinish`, `IsCritical = TotalFloat ≤ Duration.Zero`; the fold is total over the acyclic graph and carries the `Fin<T>` rail only for the cycle rejection.
- Auto: `WorkCalendar.Advance(Instant from, Duration work)` walks forward from `from` in the model zone, accumulating only the working hours each calendar day contributes (a day in the `WorkWeek` set and not in `ExceptionDays` contributes its shift-span `Duration`, every other day contributes zero) until the accumulated working `Duration` reaches `work`, so a five-working-day task starting on a Friday resolves its finish on the following Friday across the intervening weekend, and a holiday/weather exception day extends the window by one shift; `Recede` is the symmetric backward walk the backward pass reads; `WorkingSpan(LocalDate)` reads the day's shift `Duration` (zero on a non-working day) and `ExpandExceptions` folds each `ExceptionTimes` `IfcRecurrencePattern` onto the concrete `LocalDate` set through NodaTime `Period`/`LocalDate.PlusDays`/`LocalDate.Next(IsoDayOfWeek)` date arithmetic, never re-deriving day math. `ScheduleNetwork.Schedule` threads the `(forward, backward)` accumulator as two folds over the one topological order: the forward fold seeds a no-predecessor task at the project start (the minimum scheduled `Interval.Start`), reads the predecessor `CriticalPath` already computed (topological order guarantees it is present), and the backward fold seeds a no-successor task at the project finish (the maximum forward `EarlyFinish`); a task whose `SequenceRel` modality is `StartToStart`/`FinishToFinish` shifts the pass through the union arm rather than assuming finish-to-start, so the float algebra discriminates the `SequenceRel.Switch` arm. The `ScheduleNetwork.WorkingFinish(WorkCalendar)` companion re-resolves each `ConstructionTask.Scheduled` finish in working time so a stored continuous `Interval` reads its calendar-true finish, recovering the under-utilized NodaTime `ZonedDateTime`/`LocalTime`/`IsoDayOfWeek` surface beyond `Instant`.
- Receipt: the `Map<string, CriticalPath>` is the CPM evidence the `csharp:Rasm.AppUi/schedule` critical-chain report and the 4D playback read — the critical-path set is `path.Filter(static (_, cp) => cp.IsCritical)`, the per-task float window the resource-leveling read, and the `EarlyStart`/`LateFinish` the schedule bounds; the `WorkCalendar` working-time finish is the calendar-accurate `Interval` the `ConstructionState.At` snapshot and the `Planning/cost#ESTIMATE` `EarnedValue` schedule-performance index read, never a continuous-span approximation.
- Packages: GeometryGymIFC_Core, NodaTime, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new dependency modality shifts the CPM pass through the same `SequenceRel.Switch` arm the `[2]-[SCHEDULE]` union widens on; a new calendar exception kind is one `IfcRecurrencePattern` `RecurrenceType` arm the `ExpandExceptions` fold reads; a new float metric (a working-day float, a critical-chain buffer) is one column on `CriticalPath` derived from the same forward/backward pass; never a per-modality CPM pass, never a second calendar engine, and never a mutable PERT accumulator.
- Boundary: the CPM pass is ONE immutable fold over the `SequenceRel` adjacency by topological order — a mutable `Dictionary<string, double>` early-start accumulator mutated in a `for` loop is the deleted form, the forward/backward pass threads the `Map<string, CriticalPath>` accumulator through the topological fold; the topological order is Kahn's algorithm over the predecessor→successor adjacency and a cyclic dependency edge aborts `BimFault.ModelRejected` rather than looping, the `Fin<T>` rail carried only for the cycle rejection; the `WorkCalendar` working-time arithmetic composes the NodaTime `LocalDate`/`LocalTime`/`Period`/`IsoDayOfWeek`/`LocalDate.Next` surface and a hand-rolled day-counter or a BCL `DateTime.AddDays` loop is the deleted form — NodaTime owns the date arithmetic and the calendar function composes it; the `IfcRecurrencePattern` weekly/by-weekday grammar narrows to the construction work-week and exception-day cases without minting a parallel recurrence engine, the `RecurrencePattern` accessor read through the `IfcWorkTime` STEP graph because the GeometryGym getter is schema-internal; the GeometryGym `IfcWorkCalendar.WorkingTimes`/`ExceptionTimes` `SET<IfcWorkTime>`, `IfcWorkTime.RecurrencePattern`/`StartDate`/`FinishDate`, and `IfcRecurrencePattern.RecurrenceType`/`WeekdayComponent`/`DayComponent`/`Interval`/`Occurrences` member spellings are consumed as settled vocabulary; the `CriticalPath` float window is the `Planning/cost#ESTIMATE` `EarnedValue` schedule-performance read and re-deriving the activity network on the cost page is the deleted form; a CPM rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` through `.ToError()`.

```csharp contract
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record WorkCalendar(
    System.Collections.Generic.HashSet<IsoDayOfWeek> WorkWeek,
    LocalTime ShiftStart,
    LocalTime ShiftEnd,
    System.Collections.Generic.HashSet<LocalDate> ExceptionDays,
    DateTimeZone Zone) {
    public static readonly WorkCalendar Default = new(
        new() { IsoDayOfWeek.Monday, IsoDayOfWeek.Tuesday, IsoDayOfWeek.Wednesday, IsoDayOfWeek.Thursday, IsoDayOfWeek.Friday },
        new LocalTime(8, 0), new LocalTime(16, 0), new(), DateTimeZone.Utc);

    Duration ShiftSpan => (ShiftEnd - ShiftStart).ToDuration();

    Duration WorkingSpan(LocalDate day) =>
        WorkWeek.Contains(day.DayOfWeek) && !ExceptionDays.Contains(day) ? ShiftSpan : Duration.Zero;

    // Walk forward day-by-day accumulating only the working hours each calendar day contributes until
    // `work` is consumed, so a duration-driven task finishes past the intervening non-working days.
    public Instant Advance(Instant from, Duration work) {
        var cursor = from.InZone(Zone);
        var remaining = work;
        while (remaining > Duration.Zero) {
            var span = WorkingSpan(cursor.Date);
            if (span <= Duration.Zero) { cursor = StartOfNextDay(cursor); continue; }
            if (remaining <= span) { return cursor.ToInstant() + remaining; }
            remaining -= span;
            cursor = StartOfNextDay(cursor);
        }
        return cursor.ToInstant();
    }

    public Instant Recede(Instant to, Duration work) {
        var cursor = to.InZone(Zone);
        var remaining = work;
        while (remaining > Duration.Zero) {
            var span = WorkingSpan(cursor.Date);
            if (span <= Duration.Zero) { cursor = EndOfPriorDay(cursor); continue; }
            if (remaining <= span) { return cursor.ToInstant() - remaining; }
            remaining -= span;
            cursor = EndOfPriorDay(cursor);
        }
        return cursor.ToInstant();
    }

    ZonedDateTime StartOfNextDay(ZonedDateTime at) => (at.Date.PlusDays(1) + ShiftStart).InZoneLeniently(Zone);
    ZonedDateTime EndOfPriorDay(ZonedDateTime at) => (at.Date.PlusDays(-1) + ShiftEnd).InZoneLeniently(Zone);

    public static WorkCalendar Of(IfcWorkCalendar calendar, DateTimeZone zone) {
        var week = calendar.WorkingTimes.AsIterable()
            .Bind(static wt => WeekdaysOf(wt.RecurrencePattern))
            .Map(static iso => (IsoDayOfWeek)iso)
            .ToHashSet();
        var exceptions = calendar.ExceptionTimes.AsIterable()
            .Bind(ExpandExceptions)
            .ToHashSet();
        return new WorkCalendar(
            week.Count > 0 ? week : Default.WorkWeek,
            Default.ShiftStart, Default.ShiftEnd, exceptions, zone);
    }

    static Seq<int> WeekdaysOf(IfcRecurrencePattern? pattern) =>
        pattern is { WeekdayComponent.Count: > 0 } p ? p.WeekdayComponent.AsIterable().ToSeq() : Seq<int>();

    static Seq<LocalDate> ExpandExceptions(IfcWorkTime exception) {
        if (exception.StartDate == default) { return Seq<LocalDate>(); }
        var start = LocalDate.FromDateTime(exception.StartDate);
        var finish = exception.FinishDate == default ? start : LocalDate.FromDateTime(exception.FinishDate);
        var weekdays = exception.RecurrencePattern is { WeekdayComponent.Count: > 0 } p
            ? p.WeekdayComponent.AsIterable().ToSeq()
            : Seq<int>();
        int span = Period.Between(start, finish, PeriodUnits.Days).Days;
        return Range(0, span + 1)
            .Map(offset => start.PlusDays(offset))
            .Filter(day => weekdays.IsEmpty || weekdays.Contains((int)day.DayOfWeek))
            .ToSeq();
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
    public static Fin<Map<string, CriticalPath>> Schedule(this ScheduleNetwork network, WorkCalendar calendar) =>
        Topological(network).Map(order => {
            var duration = network.Tasks.ToMap(static t => t.GlobalId, t => t.Scheduled.Duration);
            var predecessors = network.Dependencies.GroupBy(static d => d.SuccessorGlobalId)
                .ToMap(static g => g.Key, static g => g.ToSeq());
            var successors = network.Dependencies.GroupBy(static d => d.PredecessorGlobalId)
                .ToMap(static g => g.Key, static g => g.ToSeq());
            var projectStart = network.Tasks.Map(static t => t.Scheduled.Start).Min();

            var forward = order.Fold(Map<string, (Instant Es, Instant Ef)>(), (acc, id) => {
                var es = predecessors.Find(id).Match(
                    Some: edges => edges.Map(e => Shift(e, acc, duration)).Max(),
                    None: () => projectStart);
                return acc.Add(id, (es, calendar.Advance(es, duration[id])));
            });
            var projectFinish = forward.Values.Map(static p => p.Ef).Max();

            var backward = order.Rev().Fold(Map<string, (Instant Ls, Instant Lf)>(), (acc, id) => {
                var lf = successors.Find(id).Match(
                    Some: edges => edges.Map(e => acc[e.SuccessorGlobalId].Ls).Min(),
                    None: () => projectFinish);
                return acc.Add(id, (calendar.Recede(lf, duration[id]), lf));
            });

            return order.Fold(Map<string, CriticalPath>(), (acc, id) => {
                var (es, ef) = forward[id];
                var (ls, lf) = backward[id];
                var free = successors.Find(id).Match(
                    Some: edges => edges.Map(e => forward[e.SuccessorGlobalId].Es).Min() - ef,
                    None: () => Duration.Zero);
                return acc.Add(id, CriticalPath.Of(es, ef, ls, lf, free));
            });
        });

    static Instant Shift(SequenceRel edge, Map<string, (Instant Es, Instant Ef)> forward, Map<string, Duration> duration) {
        var (es, ef) = forward[edge.PredecessorGlobalId];
        var lag = edge.Lag.ToDuration();
        return edge.Switch(
            finishToStart:  _ => ef + lag,
            startToStart:   _ => es + lag,
            finishToFinish: _ => ef + lag - duration[edge.SuccessorGlobalId]);
    }

    // Kahn's algorithm over the predecessor→successor adjacency; a residual non-empty node set after the
    // queue drains is a cycle the CPM pass cannot order, lowered onto BimFault.ModelRejected.
    static Fin<Seq<string>> Topological(ScheduleNetwork network) {
        var indegree = network.Tasks.ToMap(static t => t.GlobalId, _ => 0);
        var degree = network.Dependencies.Fold(indegree, static (acc, d) =>
            acc.AddOrUpdate(d.SuccessorGlobalId, static n => n + 1, 1));
        var successors = network.Dependencies.GroupBy(static d => d.PredecessorGlobalId)
            .ToMap(static g => g.Key, static g => g.Map(static e => e.SuccessorGlobalId).ToSeq());
        return Drain(
            degree.Filter(static (_, n) => n == 0).Keys.ToSeq(),
            degree, successors, Seq<string>());

        static Fin<Seq<string>> Drain(Seq<string> ready, Map<string, int> degree, Map<string, Seq<string>> successors, Seq<string> order) =>
            ready.HeadOrNone().Match(
                None: () => order.Count == degree.Count
                    ? FinSucc(order)
                    : FinFail<Seq<string>>(new BimFault.ModelRejected($"schedule-cyclic-dependency:{degree.Count - order.Count}").ToError()),
                Some: id => {
                    var relaxed = successors.Find(id).IfNone(Seq<string>())
                        .Fold(degree, static (acc, s) => acc.AddOrUpdate(s, static n => n - 1, 0));
                    var unblocked = successors.Find(id).IfNone(Seq<string>()).Filter(s => relaxed[s] == 0);
                    return Drain(ready.Tail.Concat(unblocked), relaxed, successors, order.Add(id));
                });
    }
}
```

## [04]-[RESEARCH]

- [WORK_PLAN_DISPATCH]: the `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` container traversal — the plan's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcWorkSchedule` set the `SchedulesOf` fold materializes, each schedule's controlled `IfcTask` set the `TasksOf` fold materializes, each task's `IsPredecessorTo` `IfcRelSequence` dependency set the `SequencesOf` fold reads, and each task's `OperatesOn` `IfcRelAssignsToProcess` set the `AssignmentsOf` fold reads — grounds against the GeometryGym scheduling-cost-resource family (`.api/api-geometrygym-ifc` scheduling rows 1-8) and the scheduling-traversal entrypoints (`.api/api-geometrygym-ifc` georeferencing-scheduling-grouping rows 4-6) so the `TaskOf`/`SequencesOf`/`AssignmentsOf` projections discriminate the real activity graph rather than a guessed shape; the `IfcProcess.IsPredecessorTo`/`IsSuccessorFrom` `IfcRelSequence` edge spelling, the `IfcRelSequence.RelatingProcess`/`RelatedProcess`/`TimeLag`/`SequenceType` member spellings, the `IfcTask.TaskTime`/`Status`/`IsMilestone` member spellings, and the `IfcRelAssignsToProcess.RelatingProcess`/`RelatedObjects` spellings confirm against the catalogued surface before the projection fold is final — the `IfcProcess.OperatesOn` assignment path is distinct from the `IfcWorkSchedule.Controls` `IfcRelAssignsToControl.RelatedObjects` control path the `TasksOf` fold reads the parent schedule kind through, so the task's `WorkScheduleKind` threads from the owning schedule the traversal already holds rather than a re-resolved task inverse.
- [TASK_TIME_INTERVAL]: the `IfcTaskTime` schedule/actual start-finish-duration the `IntervalOf`/`ActualOf` folds read onto the NodaTime `Interval`s — the `ScheduleStart`/`ScheduleFinish` and `ActualStart`/`ActualFinish` BCL `DateTime` members and the `ScheduleDuration` member — ground against the GeometryGym `IfcTaskTime` derivation (`.api/api-geometrygym-ifc` scheduling row 3) so the `Interval` over the work-calendar zone reads the real `DateTime?` member shape rather than a guessed one; the BCL `DateTime` lifts through `LocalDateTime.FromDateTime` and maps into the model's `DateTimeZone` through `InZoneLeniently` so an IFC calendar value carrying no offset resolves the daylight-transition gap/overlap the `NodaTime.DateTimeZone.MapLocal` ambiguous/skipped model would otherwise reject (`.api/api-nodatime` instant-local-span entrypoints rows 5-6,9-11,16), and the `Interval.Contains` membership predicate (`.api/api-nodatime` instant-local-span entrypoint row 16) backs the `ConstructionTask.ActiveAt` fold so the 4D snapshot at instant `t` reads the receipt-window membership the NodaTime `Interval` owns rather than a hand-rolled comparison — the under-utilization recovery the ledger names: today only `Instant` is exploited, this owner is the first `Interval`/`Period`/`ZonedDateTime`/`LocalDateTime` consumer.
- [LAG_PERIOD]: the `IfcRelSequence.TimeLag` `IfcLagTime` dependency lag the `PeriodOf` fold reads onto the NodaTime `Period` is verified against the live GeometryGym decompile — `IfcLagTime.LagValue` is an `IfcTimeOrRatioSelect` interface the duration leg `IfcDuration` (`: IfcSimpleValue`) implements, so the fold narrows `LagValue is IfcDuration` and reads the decomposed `Years`/`Months`/`Days`/`Hours`/`Minutes`/`Seconds` integer fields directly through a NodaTime `PeriodBuilder` (`.api/api-nodatime` `PeriodBuilder` row 14, `PeriodBuilder.Build` entrypoint row 15) rather than round-tripping the fragile `ToString()` through `PeriodPattern.Roundtrip` — `IfcDuration` is a structured measure, not a bare ISO string, so the decomposed-field build is the exact projection; the `DurationType` `IfcTaskDurationEnum` (`ELAPSEDTIME`/`WORKTIME`) interpretation rides the same `IfcLagTime` derivation; the `IfcSequenceEnum` `START_START`/`FINISH_FINISH`/`FINISH_START`/`START_FINISH` members (`.api/api-geometrygym-ifc` enum row 10) fold onto the three frozen `SequenceRel` arms through the `SequenceRel.Of` resolver, the rare `START_FINISH` and the `USERDEFINED`/`NOTDEFINED` fallbacks lowering onto `FinishToStart` so a guessed fourth modality never mints a parallel edge type.
- [SCHEDULE_CONTENT_KEY]: the `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` `UInt128` pair the `csharp:Rasm.AppUi/schedule` report and `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence read the network by grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom, so the report re-reads the network only on a changed `GeometryKey` (the assigned-element GlobalIds) or `ScheduleKey` (the task `Interval` bounds and dependency-edge lags) rather than re-projecting the container; the `ConstructionState.At(Instant)` snapshot resolves the active-element set through the `Model/query#ELEMENT_SET` `ElementSet.Where` GlobalId-membership combinator over the federated `BimModel.Elements`, so the 4D playback at a Gantt instant is one `ElementSet` value the AppUi viewport renders — re-minting a schedule-element selection surface or a `ByGlobalId` predicate arm in the sequencing folder is the no-second-selection-surface reject, and the `Planning/cost#ESTIMATE` 5D resource-join cluster reads this `ConstructionTask` time axis by the `[2]-[SCHEDULE]` anchor reference rather than re-deriving the activity network. The durable external-scheduler store `csharp:Rasm.Persistence/Sync/schedule # [WIRE]` federates its Primavera P6/MS-Project `ScheduleTask` float overlay to this host-neutral domain by the `ConstructionTask.GlobalId` — the Persistence `ScheduleTask.GlobalId` correlation joins each external activity to the IFC-projected task so a stored P6 total-float erosion overlays the IFC `CriticalPath` window by `GlobalId`, never by the external P6 `TaskId`; the two read disjoint source lanes (this owner's IFC `IfcWorkPlan` `SequenceRel` network versus the external `TaskRelation` DAG) and a `CriticalPath` here reading a P6 relation, or a Persistence `CpmPass` re-deriving this `SequenceRel` network, is the cross-package drift the `[SCHEDULE_NETWORK_DEPTH]` boundary forbids.
- [WORK_CALENDAR_RECURRENCE]: the `IfcWorkCalendar`/`IfcWorkTime`/`IfcRecurrencePattern` working-time grammar the `WorkCalendar.Of` fold reads is verified against the live GeometryGym decompile — `IfcWorkCalendar : IfcControl` exposes `WorkingTimes`/`ExceptionTimes` as `SET<IfcWorkTime>` and `PredefinedType` (`IfcWorkCalendarTypeEnum`); `IfcWorkTime : IfcSchedulingTime` carries `StartDate`/`FinishDate` (BCL `DateTime`, `DateTime.MinValue` sentinel for absent) and `RecurrencePattern` (`IfcRecurrencePattern`) — the `RecurrencePattern` getter is schema-internal in 25.7.30, so the fold reads it through the `IfcWorkTime` instance the `IfcWorkCalendar` STEP graph already holds rather than a re-resolved public inverse; `IfcRecurrencePattern : BaseClassIfc` carries `RecurrenceType` (`IfcRecurrenceTypeEnum`, default `WEEKLY`), `WeekdayComponent`/`DayComponent`/`MonthComponent` (`List<int>`, the ISO-weekday set the work-week reads), `Interval`/`Position`/`Occurrences` (`int`, `int.MinValue` sentinel), and `TimePeriods` (`LIST<IfcTimePeriod>`) — so the `WeekdaysOf`/`ExpandExceptions` folds read the real `WeekdayComponent` ISO-weekday list and the recurrence window the `StartDate`/`FinishDate` span declares, the NodaTime `LocalDate.FromDateTime`/`PlusDays`/`DayOfWeek` arithmetic owning the day math and the `IfcRecurrencePattern` grammar narrowing to the work-week and exception-day cases without a parallel recurrence engine; the `IfcWorkCalendar.WorkingTimes`/`ExceptionTimes` and the `IfcWorkCalendar`/`IfcWorkTime`/`IfcRecurrencePattern` rows the `.api/api-geometrygym-ifc` scheduling-cost-resource family (row 6) catalogues confirm the projection before the `WorkCalendar` fold is final.
- [CPM_TOPOLOGICAL_PASS]: the `ScheduleNetwork.Schedule` forward/backward-pass CPM fold runs Kahn's topological sort over the `SequenceRel` predecessor→successor adjacency — the forward pass folds each task in topological order deriving `EarlyStart` as the maximum over predecessors of the modality-shifted predecessor finish plus the edge `Period` lag (the `SequenceRel.Switch` discriminating `FinishToStart`/`StartToStart`/`FinishToFinish` so the float algebra never assumes finish-to-start) and `EarlyFinish` through `WorkCalendar.Advance`, the backward pass folds in reverse topological order deriving `LateFinish`/`LateStart` through `WorkCalendar.Recede`, and `TotalFloat = LateStart − EarlyStart`, `FreeFloat` against the earliest successor `EarlyStart`, `IsCritical` on zero total float — the standard CPM algorithm grounded against the buildingSMART scheduling vocabulary and the NodaTime `Instant`/`Duration` arithmetic, the topological order making a single pass over each task sufficient (a predecessor's `CriticalPath` is computed before its successor) and a residual cycle the Kahn drain cannot order lowering `BimFault.ModelRejected` rather than looping; the `WorkCalendar` working-time `Advance`/`Recede` resolve the duration-driven finish past the non-working days so a five-working-day task spanning a weekend reads its calendar-true finish, recovering the under-utilized NodaTime `ZonedDateTime`/`LocalTime`/`IsoDayOfWeek` surface beyond `Instant`.
