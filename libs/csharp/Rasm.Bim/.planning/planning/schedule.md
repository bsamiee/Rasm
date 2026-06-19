# [BIM_SCHEDULE]

The host-neutral 4D construction-sequencing projection: one `ScheduleNetwork` record carrying the activity network — the `ConstructionTask` row whose `IfcTaskTime` schedule and actual start/finish fold onto NodaTime `Interval`s over the work-calendar zone, the closed `SequenceRel` `[Union]` (`FinishToStart`/`StartToStart`/`FinishToFinish`) carrying each `IfcRelSequence` dependency lag as a NodaTime `Period`, the `TaskAssignment` projecting `IfcRelAssignsToProcess` to join a task to the `Model/query#ELEMENT_SET`-selected `BimElement` set, and the `ScheduleProjection.Project` fold that folds the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` container into the typed network — plus the `ConstructionState.At(Instant)` snapshot that reads the element set whose task `Interval.Contains` the queried instant. The schedule is a VIEW of the federated `Model/elements#ELEMENT_MODEL` `BimModel`, never a re-modeled element graph: a task carries its assigned-element GlobalIds, the snapshot resolves them against the one `BimModel.Elements` index, and the `csharp:Rasm.AppUi/schedule` schedule report and the `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence consume the typed network by reference. The schedule is HOST-NEUTRAL — the calendar values ride NodaTime `Interval`/`Period`/`ZonedDateTime` over the model's work-calendar zone and never a BCL `DateTime` on a public signature — and the task-to-element join is the `Model/query#ELEMENT_SET` `ElementPredicate` algebra selecting the assigned set, never a second selection surface. The activity network is the `Planning/cost#ESTIMATE` 5D pairing's time axis: a `ConstructionResource` joins `sequencing#ConstructionTask` by its `GlobalId` and the `CostSchedule.Rollup` reads the task `Interval` the schedule owns, so this page authors the `[2]-[SCHEDULE]` `ConstructionTask` anchor the cost resource-join cluster cites by reference rather than re-deriving the activity network. A schedule rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` via `.ToError()`, never a new fault family — a task naming an assigned-element GlobalId the federated model never declares is `BimFault.DanglingReference`, a dependency edge naming an absent process is `BimFault.ModelRejected`, never a sixth arm.

## [01]-[INDEX]

- [01]-[SCHEDULE]: `ScheduleNetwork` record, the `ConstructionTask` record (`IfcTaskTime` as a NodaTime `Interval`), the `SequenceRel` `[Union]` (`FinishToStart`/`StartToStart`/`FinishToFinish`) with `Period` lag, the `TaskAssignment` record, the `TaskStatus`/`WorkScheduleKind` `[SmartEnum]` vocabularies, the `ScheduleProjection.Project` fold from the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` surface, and the `ConstructionState.At(Instant)` element-set snapshot.

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

## [03]-[RESEARCH]

- [WORK_PLAN_DISPATCH]: the `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask` container traversal — the plan's `Controls` `IfcRelAssignsToControl.RelatedObjects` controlled `IfcWorkSchedule` set the `SchedulesOf` fold materializes, each schedule's controlled `IfcTask` set the `TasksOf` fold materializes, each task's `IsPredecessorTo` `IfcRelSequence` dependency set the `SequencesOf` fold reads, and each task's `OperatesOn` `IfcRelAssignsToProcess` set the `AssignmentsOf` fold reads — grounds against the GeometryGym scheduling-cost-resource family (`.api/api-geometrygym-ifc` scheduling rows 1-8) and the scheduling-traversal entrypoints (`.api/api-geometrygym-ifc` georeferencing-scheduling-grouping rows 4-6) so the `TaskOf`/`SequencesOf`/`AssignmentsOf` projections discriminate the real activity graph rather than a guessed shape; the `IfcProcess.IsPredecessorTo`/`IsSuccessorFrom` `IfcRelSequence` edge spelling, the `IfcRelSequence.RelatingProcess`/`RelatedProcess`/`TimeLag`/`SequenceType` member spellings, the `IfcTask.TaskTime`/`Status`/`IsMilestone` member spellings, and the `IfcRelAssignsToProcess.RelatingProcess`/`RelatedObjects` spellings confirm against the catalogued surface before the projection fold is final — the `IfcProcess.OperatesOn` assignment path is distinct from the `IfcWorkSchedule.Controls` `IfcRelAssignsToControl.RelatedObjects` control path the `TasksOf` fold reads the parent schedule kind through, so the task's `WorkScheduleKind` threads from the owning schedule the traversal already holds rather than a re-resolved task inverse.
- [TASK_TIME_INTERVAL]: the `IfcTaskTime` schedule/actual start-finish-duration the `IntervalOf`/`ActualOf` folds read onto the NodaTime `Interval`s — the `ScheduleStart`/`ScheduleFinish` and `ActualStart`/`ActualFinish` BCL `DateTime` members and the `ScheduleDuration` member — ground against the GeometryGym `IfcTaskTime` derivation (`.api/api-geometrygym-ifc` scheduling row 3) so the `Interval` over the work-calendar zone reads the real `DateTime?` member shape rather than a guessed one; the BCL `DateTime` lifts through `LocalDateTime.FromDateTime` and maps into the model's `DateTimeZone` through `InZoneLeniently` so an IFC calendar value carrying no offset resolves the daylight-transition gap/overlap the `NodaTime.DateTimeZone.MapLocal` ambiguous/skipped model would otherwise reject (`.api/api-nodatime` instant-local-span entrypoints rows 5-6,9-11,16), and the `Interval.Contains` membership predicate (`.api/api-nodatime` instant-local-span entrypoint row 16) backs the `ConstructionTask.ActiveAt` fold so the 4D snapshot at instant `t` reads the receipt-window membership the NodaTime `Interval` owns rather than a hand-rolled comparison — the under-utilization recovery the ledger names: today only `Instant` is exploited, this owner is the first `Interval`/`Period`/`ZonedDateTime`/`LocalDateTime` consumer.
- [LAG_PERIOD]: the `IfcRelSequence.TimeLag` `IfcLagTime` dependency lag the `PeriodOf` fold reads onto the NodaTime `Period` is verified against the live GeometryGym decompile — `IfcLagTime.LagValue` is an `IfcTimeOrRatioSelect` interface the duration leg `IfcDuration` (`: IfcSimpleValue`) implements, so the fold narrows `LagValue is IfcDuration` and reads the decomposed `Years`/`Months`/`Days`/`Hours`/`Minutes`/`Seconds` integer fields directly through a NodaTime `PeriodBuilder` (`.api/api-nodatime` `PeriodBuilder` row 14, `PeriodBuilder.Build` entrypoint row 15) rather than round-tripping the fragile `ToString()` through `PeriodPattern.Roundtrip` — `IfcDuration` is a structured measure, not a bare ISO string, so the decomposed-field build is the exact projection; the `DurationType` `IfcTaskDurationEnum` (`ELAPSEDTIME`/`WORKTIME`) interpretation rides the same `IfcLagTime` derivation; the `IfcSequenceEnum` `START_START`/`FINISH_FINISH`/`FINISH_START`/`START_FINISH` members (`.api/api-geometrygym-ifc` enum row 10) fold onto the three frozen `SequenceRel` arms through the `SequenceRel.Of` resolver, the rare `START_FINISH` and the `USERDEFINED`/`NOTDEFINED` fallbacks lowering onto `FinishToStart` so a guessed fourth modality never mints a parallel edge type.
- [SCHEDULE_CONTENT_KEY]: the `ScheduleNetwork.Identity` `(GeometryKey, ScheduleKey)` `UInt128` pair the `csharp:Rasm.AppUi/schedule` report and `csharp:Rasm.Persistence/Query/federation#FEDERATION` activity-network persistence read the network by grounds against the `Review/diff#MODEL_DIFF` `XxHash128.HashToUInt128` content-key idiom, so the report re-reads the network only on a changed `GeometryKey` (the assigned-element GlobalIds) or `ScheduleKey` (the task `Interval` bounds and dependency-edge lags) rather than re-projecting the container; the `ConstructionState.At(Instant)` snapshot resolves the active-element set through the `Model/query#ELEMENT_SET` `ElementSet.Where` GlobalId-membership combinator over the federated `BimModel.Elements`, so the 4D playback at a Gantt instant is one `ElementSet` value the AppUi viewport renders — re-minting a schedule-element selection surface or a `ByGlobalId` predicate arm in the sequencing folder is the no-second-selection-surface reject, and the `Planning/cost#ESTIMATE` 5D resource-join cluster reads this `ConstructionTask` time axis by the `[2]-[SCHEDULE]` anchor reference rather than re-deriving the activity network.
