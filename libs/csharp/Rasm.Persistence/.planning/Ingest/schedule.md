# [PERSISTENCE_INGEST_SCHEDULE]

Rasm.Persistence ingests and emits project-schedule files through ONE `ScheduleSource` owner over the `MPXJ.Net` interchange codec: ~20 read dialects (P6 `XER`/`PMXML`, MS Project `.mpp`/`MSPDI`/`MPX`, Asta, Phoenix, Planner, SDEF, …) materialize into the neutral `ProjectFile` graph through ONE format-sniffing ingress — `new UniversalProjectReader().ReadAll(string|Stream)` — and the WRITE half serializes back OUT through ONE egress — `new UniversalProjectWriter(FileFormat).Write(ProjectFile|IList<ProjectFile>, …)` — over the seven writable `FileFormat` members the `ScheduleFormat` `[SmartEnum<string>]` freezes, so the durable store round-trips a P6/MS-Project schedule in both directions and never truncates a multi-project XER container: the ingress is ALWAYS `ReadAll` (a single-project file is the one-element container, arity recoverable from the yield, never a `multi` knob), and the egress arity is the graph's own count (`Write(file)` at one, `Write(files, …)` past one). This persisted payload is the durable activity-network DAG a scheduling store actually keeps — the full `ScheduleActivity` row (schedule/actual/baseline windows, duration/work/cost, slack, constraint, WBS and activity ids, critical/milestone flags), the predecessor/successor/`DependencyKind`/lag `TaskRelation` rows (the CPM edge set), the working-time `WorkCalendarRow` weekly pattern with date-ranged `WeekRow` overrides and per-exception shift windows, and the `ResourceRow`/`ResourceAvailabilityRow`/`AssignmentRow` loading and capacity — reconstructed on the write leg through `Relation.Builder(ProjectFile)`, `Duration.GetInstance(double, TimeUnit)`, `Availability(DateTime?, DateTime?, double?)`, and the calendar day/hour surface, so the store's rows, not a retained foreign object, are the system of record.

This codec NEVER knows the element graph and NEVER computes schedule math: the per-app schedule→element map (the wire-composition owner at the host/app composition root, the `ARCHITECTURE.md` `[02]-[SEAMS]` `Ingest → Rasm.Element` row-shape law) projects each activity row into a `Rasm.Element` graph node, and the CPM forward/backward pass, resource leveling, and 4D sequencing live in `Rasm.Bim` (the `TaskRelation` rows are exactly the `SequenceRel` DAG its QuikGraph `SourceFirstBidirectionalTopologicalSort` orders — the `Rasm.Bim/Planning/schedule` counterpart, widened read→round-trip along the `[02]-[SEAMS]` `Ingest ↔ Rasm.Bim` `TaskRelation` wire). `ScheduleRows.Reconcile` correlates a fresh progress-update import by `ActivityId`, then WBS, then file key and yields `ScheduleVariance` over every durable axis: activity scope and value changes, relation topology and lag, assignment loading, calendar patterns and recurrence, resource economics, and project anchors. This IKVM boundary is ONE seam: every `MPXJ.Net` proxy carries a `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>`, and `ProjectRows.Of`/`Synthesis.Fold` are the only members that touch a proxy type — the handle never threads into a durable row, durations and lags cross unit-tagged as `Option<ScheduleSpan>` through the closed `ScheduleUnit` vocabulary, local wall stamps cross as NodaTime `LocalDateTime`/`LocalDate`, and the four `RelationType` members cross once through `DependencyKind` with the ten `ConstraintType` members crossing through `ConstraintKind`. Both legs fold every codec exception through one `ScheduleFault.Lift` funnel into `Validation<ScheduleFault, …>` at the row boundary — a null `ReadAll` yield rails `UnknownDialect`, an absent requested project rails `ProjectMissing`, a codec throw rails `CodecReject`, and `Semigroup`/`Aggregate` keeps accumulation manufacturable — with `Code => FaultBand.Schedule + n` off the `Element/graph#FAULT_TABLES` registry and every fact riding the `ScheduleFactKind`-discriminated stream under `store.schedule.*`. `Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_RAIL`; `FaultBand` from `Element/graph#FAULT_TABLES`; `ReceiptSinkPort` from AppHost.

## [01]-[INDEX]

- [01]-[SCHEDULE_SOURCE]: the one parse/serialize/probe op family over `ScheduleSpec`, the seven-member `ScheduleFormat` egress axis, the `DependencyKind`/`ConstraintKind` seam vocabularies, the always-`ReadAll` container law, the row-boundary fault fold on both legs, and the typed fact stream.
- [02]-[DURABLE_NETWORK]: the persisted activity/relation/calendar/resource rows at full MPXJ axis depth, the unit-tagged `Option<ScheduleSpan>` crossing, the `ProjectRows.Of`/`Synthesis.Fold` round-trip seam, and the `ScheduleRows.Reconcile` baseline/update variance owner.

## [02]-[SCHEDULE_SOURCE]

- Owner: `ScheduleFormat` the `[SmartEnum<string>]` egress axis frozen to the seven writable `FileFormat` members; `DependencyKind` the four CPM dependency modalities carrying `RelationType`; `ConstraintKind` the ten constraint modalities carrying `ConstraintType`; `ScheduleUnit` the fourteen unit and elapsed-unit rows carrying `TimeUnit`; `ScheduleSpec` the `[ComplexValueObject]` fixing `Origin` plus the optional project selector; `ScheduleOp` the closed `Parse | Serialize | Probe` family; `ScheduleYield` the closed `Projects | Written | Profile` result; `ScheduleFault` the closed row-boundary fault band; `ScheduleFactKind` the closed receipt vocabulary; `ScheduleFact` the receipt record; `ScheduleSource` the one `Run` dispatch.
- Cases: `ScheduleOp.Parse(ScheduleSpec)` reads the container through `ReadAll`, validates the selector, and projects each `ProjectFile` into `ScheduleProject`; `ScheduleOp.Serialize(ScheduleSpec, ScheduleFormat, Seq<ScheduleProject>)` synthesizes the durable rows and writes the target format; `ScheduleOp.Probe(ScheduleSpec)` yields the per-project `ScheduleProfile` roster; `ScheduleFault` is `CodecReject | UnknownDialect | ProjectMissing | Aggregate`; `ScheduleFactKind` is `Parse | Write | Probe`.
- Entry: `public static IO<Validation<ScheduleFault, ScheduleYield>> Run(ScheduleOp op, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink)` — ONE polymorphic entry discriminating the closed op union through the generated total `Switch` (a new modality is one case that breaks this dispatch at compile time), folding both legs through the `Capture` funnel so the receipt path never sees a thrown codec exception.
- Auto: the parse leg opens the `Origin` (path or caller-owned stream) once, calls `ReadAll` — NEVER the single-project `Read`, which silently truncates a multi-project XER to its first project, and NEVER an extension branch, because the reader format-sniffs — then projects each `ProjectFile` through `ProjectRows.Of`, the ONE IKVM seam: activities from the flat `Tasks` container with the WBS parent threaded from `ChildTasks` reachability, the dependency network from each `Task.Predecessors` (`IList<Relation>` — reading one side of the symmetric pair so an edge lands once), the full calendar record (weekly day pattern via `GetCalendarDayType`/`GetCalendarHours`, `WorkWeeks` overrides, per-exception shift windows), resources and loading from `Resources`/`ResourceAssignments`; the serialize leg folds each `ScheduleProject` through `Synthesis.Fold` — anchor properties re-stamped, calendars rebuilt day-by-day (`SetWorkingDay`/`AddCalendarHours`/`AddWorkWeek`/`AddCalendarException` with each PERSISTED shift range re-added, never a fixed default shift), WBS children minted THROUGH their parent (`Task.AddTask()`), `Relation.Builder(file).PredecessorTask(pred).SuccessorTask(succ).Type(kind.Wire)` per relation with `.Lag(lag.Wire)` applied only on a present lag, `AddResource` and `Task.AddResourceAssignment(Resource)` per loading row — then writes through ONE `UniversalProjectWriter(to.Wire)` whose arity is the graph count; the probe leg reads `ProjectProperties` (`FileType` the parsed source dialect, `FileApplication`, `ProjectTitle`) plus the container's task/relation counts, so a dialect census never pays `ProjectRows.Of` projection.
- Receipt: every op rides a `ScheduleFact` through the `ReceiptSinkPort` envelope under `store.schedule.*` — a `parse` fact carrying the source dialect, project count, and activity/relation totals; a `write` fact carrying the target format key, project count, and activity/relation totals; a `probe` fact carrying the dialect roster size — one kind-discriminated stream, never parallel receipt records; the envelope stamps the HLC, the fact carrying `frame.Now()` as its own observation instant.
- Packages: MPXJ.Net (`UniversalProjectReader.ReadAll`, `UniversalProjectWriter(FileFormat).Write` single and `IList` arities, `FileFormat`, `ProjectFile.Tasks`/`ChildTasks`/`Calendars`/`Resources`/`ResourceAssignments`/`ProjectProperties`/`AddTask`/`AddResource`/`AddCalendar`/`GetTaskByUniqueID`/`GetResourceByUniqueID`, `Task` schedule/early/late/actual/baseline/constraint/WBS accessors + `AddResourceAssignment`, `Relation.Builder`/`PredecessorTask`/`SuccessorTask`/`Type`/`Lag`, `RelationType`, `ConstraintType`, `Duration.DurationValue`/`Units`/`GetInstance`, all `TimeUnit` rows, `ProjectCalendar.CalendarExceptions`/`WorkWeeks`/`Type`/`AddWorkWeek`/`AddCalendarException`, `ProjectCalendarDays.GetCalendarDayType`/`GetCalendarHours`/`SetWorkingDay`/`AddCalendarHours`, `DayType`, `TimeOnlyRange`, `DateOnlyRange`, `ResourceAssignment.Units`/`Work`/`Cost`/`BudgetCost`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Element/graph#STORE_RAIL` `ProjectionContext`, `Ingest/tabular#TABULAR_SOURCE` `Origin`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new writable dialect is one `ScheduleFormat` row carrying its `FileFormat` member (the read side grows upstream, zero rows here); a new durable axis is one row or member on the `#DURABLE_NETWORK` family; a new dependency semantics is one `DependencyKind` row; a new op modality is one `ScheduleOp` case breaking `Run` at compile time; a new boundary-fault class is one `ScheduleFault` case inside the registry decade; zero new surface — a hand-rolled XER/MSPDI parser, an extension-branched ingress, a `Read`-only lane that truncates containers, a parallel `ReadSchedule`/`WriteSchedule` name family beside the op union, CPM/leveling math inside the codec, or a schedule→element map inside this page is the deleted form because MPXJ owns parse/serialize, `Rasm.Bim` owns the schedule math, the op union owns modality, and the app composition root owns the element projection.
- Boundary: `ScheduleSource` is the ONE schedule-file ingress/egress owner; spreadsheet/delimited lanes cannot parse binary MPP or P6 XER, and both codecs project into the same downstream record rail. Ingress always uses format-sniffed `ReadAll`; writes target the seven `FileFormat` members only. IKVM proxies remain inside `ProjectRows`/`Synthesis`; `ScheduleSpan` carries `ScheduleUnit`, absent durations remain `None`, and working-day arithmetic reads calendar rows. `← Rasm.Bim/Planning/schedule` consumes `TaskRelation` and `ScheduleSpan` for 4D/5D, and `→ Rasm.Element` receives row shape only.

```csharp signature
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;
using MpxjDuration = MPXJ.Net.Duration;

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScheduleFormat {
    public static readonly ScheduleFormat Xer = new("xer", FileFormat.XER);
    public static readonly ScheduleFormat Pmxml = new("pmxml", FileFormat.PMXML);
    public static readonly ScheduleFormat Mspdi = new("mspdi", FileFormat.MSPDI);
    public static readonly ScheduleFormat Mpx = new("mpx", FileFormat.MPX);
    public static readonly ScheduleFormat Json = new("json", FileFormat.JSON);
    public static readonly ScheduleFormat Planner = new("planner", FileFormat.PLANNER);
    public static readonly ScheduleFormat Sdef = new("sdef", FileFormat.SDEF);
    public FileFormat Wire { get; }
    private ScheduleFormat(string key, FileFormat wire) : this(key) => Wire = wire;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScheduleDirection {
    public static readonly ScheduleDirection Forward = new("forward", ScheduleFrom.Start);
    public static readonly ScheduleDirection Backward = new("backward", ScheduleFrom.Finish);
    public ScheduleFrom Wire { get; }
    private ScheduleDirection(string key, ScheduleFrom wire) : this(key) => Wire = wire;
    public static Option<ScheduleDirection> Of(ScheduleFrom? wire) => wire switch {
        ScheduleFrom.Start => Some(Forward),
        ScheduleFrom.Finish => Some(Backward),
        _ => None,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CalendarKind {
    public static readonly CalendarKind Global = new("global", CalendarType.Global);
    public static readonly CalendarKind Project = new("project", CalendarType.Project);
    public static readonly CalendarKind Resource = new("resource", CalendarType.Resource);
    public CalendarType Wire { get; }
    private CalendarKind(string key, CalendarType wire) : this(key) => Wire = wire;
    public static Option<CalendarKind> Of(CalendarType? wire) => wire switch {
        CalendarType.Global => Some(Global),
        CalendarType.Project => Some(Project),
        CalendarType.Resource => Some(Resource),
        _ => None,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ResourceKind {
    public static readonly ResourceKind Material = new("material", ResourceType.Material);
    public static readonly ResourceKind Work = new("work", ResourceType.Work);
    public static readonly ResourceKind Cost = new("cost", ResourceType.Cost);
    public static readonly ResourceKind NonLabor = new("non-labor", ResourceType.NonLabor);
    public ResourceType Wire { get; }
    private ResourceKind(string key, ResourceType wire) : this(key) => Wire = wire;
    public static Option<ResourceKind> Of(ResourceType? wire) => wire switch {
        ResourceType.Material => Some(Material),
        ResourceType.Work => Some(Work),
        ResourceType.Cost => Some(Cost),
        ResourceType.NonLabor => Some(NonLabor),
        _ => None,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RecurrenceKind {
    public static readonly RecurrenceKind Daily = new("daily", RecurrenceType.Daily);
    public static readonly RecurrenceKind Weekly = new("weekly", RecurrenceType.Weekly);
    public static readonly RecurrenceKind Monthly = new("monthly", RecurrenceType.Monthly);
    public static readonly RecurrenceKind Yearly = new("yearly", RecurrenceType.Yearly);
    public RecurrenceType Wire { get; }
    private RecurrenceKind(string key, RecurrenceType wire) : this(key) => Wire = wire;
    public static RecurrenceKind Of(RecurrenceType? wire) => wire switch {
        RecurrenceType.Weekly => Weekly,
        RecurrenceType.Monthly => Monthly,
        RecurrenceType.Yearly => Yearly,
        _ => Daily,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DependencyKind {
    public static readonly DependencyKind FinishStart = new("finish-start", RelationType.FinishStart);
    public static readonly DependencyKind StartStart = new("start-start", RelationType.StartStart);
    public static readonly DependencyKind FinishFinish = new("finish-finish", RelationType.FinishFinish);
    public static readonly DependencyKind StartFinish = new("start-finish", RelationType.StartFinish);
    public RelationType Wire { get; }
    private DependencyKind(string key, RelationType wire) : this(key) => Wire = wire;

    public static DependencyKind Of(RelationType? wire) => wire switch {
        RelationType.StartStart => StartStart,
        RelationType.FinishFinish => FinishFinish,
        RelationType.StartFinish => StartFinish,
        _ => FinishStart,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConstraintKind {
    public static readonly ConstraintKind AsSoonAsPossible = new("as-soon-as-possible", ConstraintType.AsSoonAsPossible);
    public static readonly ConstraintKind AsLateAsPossible = new("as-late-as-possible", ConstraintType.AsLateAsPossible);
    public static readonly ConstraintKind MustStartOn = new("must-start-on", ConstraintType.MustStartOn);
    public static readonly ConstraintKind MustFinishOn = new("must-finish-on", ConstraintType.MustFinishOn);
    public static readonly ConstraintKind StartNoEarlierThan = new("start-no-earlier-than", ConstraintType.StartNoEarlierThan);
    public static readonly ConstraintKind StartNoLaterThan = new("start-no-later-than", ConstraintType.StartNoLaterThan);
    public static readonly ConstraintKind FinishNoEarlierThan = new("finish-no-earlier-than", ConstraintType.FinishNoEarlierThan);
    public static readonly ConstraintKind FinishNoLaterThan = new("finish-no-later-than", ConstraintType.FinishNoLaterThan);
    public static readonly ConstraintKind StartOn = new("start-on", ConstraintType.StartOn);
    public static readonly ConstraintKind FinishOn = new("finish-on", ConstraintType.FinishOn);
    public ConstraintType Wire { get; }
    private ConstraintKind(string key, ConstraintType wire) : this(key) => Wire = wire;

    static readonly Lazy<FrozenDictionary<ConstraintType, ConstraintKind>> ByWire =
        new(static () => Items.ToFrozenDictionary(static row => row.Wire));

    public static Option<ConstraintKind> Of(ConstraintType? wire) =>
        wire is { } value && ByWire.Value.TryGetValue(value, out ConstraintKind? row) ? Optional(row) : None;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScheduleUnit {
    public static readonly ScheduleUnit Minutes = new("minutes", TimeUnit.Minutes);
    public static readonly ScheduleUnit Hours = new("hours", TimeUnit.Hours);
    public static readonly ScheduleUnit Days = new("days", TimeUnit.Days);
    public static readonly ScheduleUnit Weeks = new("weeks", TimeUnit.Weeks);
    public static readonly ScheduleUnit Months = new("months", TimeUnit.Months);
    public static readonly ScheduleUnit Years = new("years", TimeUnit.Years);
    public static readonly ScheduleUnit Percent = new("percent", TimeUnit.Percent);
    public static readonly ScheduleUnit ElapsedMinutes = new("elapsed-minutes", TimeUnit.ElapsedMinutes);
    public static readonly ScheduleUnit ElapsedHours = new("elapsed-hours", TimeUnit.ElapsedHours);
    public static readonly ScheduleUnit ElapsedDays = new("elapsed-days", TimeUnit.ElapsedDays);
    public static readonly ScheduleUnit ElapsedWeeks = new("elapsed-weeks", TimeUnit.ElapsedWeeks);
    public static readonly ScheduleUnit ElapsedMonths = new("elapsed-months", TimeUnit.ElapsedMonths);
    public static readonly ScheduleUnit ElapsedYears = new("elapsed-years", TimeUnit.ElapsedYears);
    public static readonly ScheduleUnit ElapsedPercent = new("elapsed-percent", TimeUnit.ElapsedPercent);

    public TimeUnit Wire { get; }

    private ScheduleUnit(string key, TimeUnit wire) : this(key) => Wire = wire;

    static readonly Lazy<FrozenDictionary<TimeUnit, ScheduleUnit>> ByWire =
        new(static () => Items.ToFrozenDictionary(static row => row.Wire));

    public static ScheduleUnit Of(TimeUnit? wire) =>
        wire is { } value && ByWire.Value.TryGetValue(value, out ScheduleUnit? unit) ? unit : Days;
}

// --- [MODELS] ---------------------------------------------------------------------------

public readonly record struct ScheduleSpan(double Magnitude, ScheduleUnit Unit) {
    public static Option<ScheduleSpan> From(MpxjDuration? span) =>
        span is null ? None : Some(new ScheduleSpan(span.DurationValue, ScheduleUnit.Of(span.Units)));
    public MpxjDuration Wire => MpxjDuration.GetInstance(Magnitude, Unit.Wire);
}

public readonly record struct ShiftRow(TimeOnly From, TimeOnly To);

public readonly record struct DayRow(DayOfWeek Day, bool Working, Seq<ShiftRow> Shifts);

public readonly record struct WeekRow(string Name, Option<LocalDate> From, Option<LocalDate> To, Seq<DayRow> Days);

public sealed record CalendarRecurrence(
    RecurrenceKind Kind,
    Option<LocalDate> Start,
    Option<LocalDate> Finish,
    Option<int> Occurrences,
    bool UseEndDate,
    bool WorkingDaysOnly,
    bool Relative,
    Option<int> Frequency,
    Option<DayOfWeek> Day,
    Option<int> DayNumber,
    Option<int> MonthNumber,
    Seq<DayOfWeek> WeeklyDays) {
    public RecurringData Wire() {
        RecurringData recurrence = new() {
            RecurrenceType = Kind.Wire,
            UseEndDate = UseEndDate,
            WorkingDaysOnly = WorkingDaysOnly,
            Relative = Relative,
        };
        Start.Iter(date => recurrence.StartDate = date.ToDateOnly());
        Finish.Iter(date => recurrence.FinishDate = date.ToDateOnly());
        Occurrences.Iter(count => recurrence.Occurrences = count);
        Frequency.Iter(frequency => recurrence.Frequency = frequency);
        Day.Iter(day => recurrence.DayOfWeek = day);
        DayNumber.Iter(day => recurrence.DayNumber = day);
        MonthNumber.Iter(month => recurrence.MonthNumber = month);
        WeeklyDays.Iter(day => recurrence.SetWeeklyDay(day, true));
        return recurrence;
    }
}

public readonly record struct CalendarException(
    string Name, Option<LocalDate> From, Option<LocalDate> To, Option<CalendarRecurrence> Recurrence, Seq<ShiftRow> Shifts) {
    public bool Working => !Shifts.IsEmpty;
}

public sealed record WorkCalendarRow(int Key, string Name, Option<CalendarKind> Kind, Seq<DayRow> Week, Seq<WeekRow> Overrides, Seq<CalendarException> Exceptions);

public sealed record ScheduleActivity(
    int Key, Option<int> Parent, string Name, Option<string> Wbs, Option<string> ActivityId,
    double Percent, bool Critical, bool Milestone, bool Summary, Option<int> OutlineLevel,
    Option<LocalDateTime> Start, Option<LocalDateTime> Finish,
    Option<LocalDateTime> EarlyStart, Option<LocalDateTime> EarlyFinish,
    Option<LocalDateTime> LateStart, Option<LocalDateTime> LateFinish,
    Option<LocalDateTime> ActualStart, Option<LocalDateTime> ActualFinish,
    Option<LocalDateTime> BaselineStart, Option<LocalDateTime> BaselineFinish,
    Option<ScheduleSpan> Duration, Option<ScheduleSpan> Work, Option<double> Cost, Option<double> BudgetCost,
    Option<ScheduleSpan> ActualDuration, Option<ScheduleSpan> ActualWork, Option<double> ActualCost,
    Option<ScheduleSpan> BaselineDuration, Option<ScheduleSpan> BaselineWork, Option<double> BaselineCost,
    Option<ScheduleSpan> PlannedDuration, Option<ScheduleSpan> PlannedWork, Option<double> PlannedCost,
    Option<ScheduleSpan> RemainingDuration, Option<ScheduleSpan> RemainingWork, Option<double> RemainingCost,
    Option<ScheduleSpan> TotalSlack, Option<ScheduleSpan> FreeSlack,
    Option<ConstraintKind> Constraint, Option<LocalDateTime> ConstraintAt) {
    public string Correlation => ActivityId.Filter(static id => !string.IsNullOrWhiteSpace(id)).Match(
        Some: static id => $"activity:{id}",
        None: () => Wbs.Filter(static wbs => !string.IsNullOrWhiteSpace(wbs)).Match(
            Some: static wbs => $"wbs:{wbs}",
            None: () => $"key:{Key}"));
}

public readonly record struct TaskRelation(int Predecessor, int Successor, DependencyKind Kind, Option<ScheduleSpan> Lag);

public readonly record struct ResourceAvailabilityRow(
    Option<LocalDateTime> From, Option<LocalDateTime> To, Option<double> Units);

public readonly record struct ResourceRow(
    int Key, string Name, Option<string> Group, Option<ResourceKind> Kind,
    Option<double> PeakUnits, Seq<ResourceAvailabilityRow> Availability,
    Option<int> Calendar, Option<double> Cost, Option<double> ActualCost, Option<double> OvertimeCost);

public readonly record struct AssignmentRow(
    int Activity, int Resource, Option<double> Units,
    Option<ScheduleSpan> Work, Option<double> Cost, Option<double> BudgetCost,
    Option<ScheduleSpan> ActualWork, Option<double> ActualCost,
    Option<ScheduleSpan> RemainingWork, Option<double> RemainingCost);

public sealed record ScheduleAnchor(
    string Dialect, string Application, string Title, Option<ScheduleDirection> Direction,
    Option<LocalDateTime> Start, Option<LocalDateTime> Finish, Option<LocalDateTime> Status, Option<LocalDateTime> Current,
    Option<string> Currency, Option<int> DefaultCalendar, Option<int> MinutesPerDay, Option<int> DaysPerMonth);

public sealed record ScheduleProject(
    ScheduleAnchor Anchor, Seq<ScheduleActivity> Activities, Seq<TaskRelation> Relations,
    Seq<WorkCalendarRow> Calendars, Seq<ResourceRow> Resources, Seq<AssignmentRow> Assignments);

public readonly record struct ScheduleProfile(string Dialect, string Application, string Title, int Activities, int Relations);

[ComplexValueObject]
public sealed partial class ScheduleSpec {
    public Origin Source { get; }
    public Option<string> Project { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Origin source, ref Option<string> project) {
        if (source is Origin.FromPath { Path: string path } && string.IsNullOrWhiteSpace(path)) {
            validationError = ValidationError.Create("<schedule-spec-path>");
        } else if (project.Map(string.IsNullOrWhiteSpace).IfNone(false)) {
            validationError = ValidationError.Create("<schedule-spec-project>");
        }
    }

    public Validation<ScheduleFault, Seq<ProjectFile>> Selected(Seq<ProjectFile> container) => Project.Match(
        Some: title => {
            Seq<ProjectFile> selected = container.Filter(project => project.ProjectProperties.ProjectTitle == title);
            return selected.IsEmpty
                ? (Validation<ScheduleFault, Seq<ProjectFile>>)new ScheduleFault.ProjectMissing(title)
                : selected;
        },
        None: () => (Validation<ScheduleFault, Seq<ProjectFile>>)container);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScheduleOp {
    private ScheduleOp() { }
    public sealed record Parse(ScheduleSpec Spec) : ScheduleOp;
    public sealed record Serialize(ScheduleSpec Target, ScheduleFormat To, Seq<ScheduleProject> Graph) : ScheduleOp;
    public sealed record Probe(ScheduleSpec Spec) : ScheduleOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScheduleYield {
    private ScheduleYield() { }
    public sealed record Projects(Seq<ScheduleProject> Rows) : ScheduleYield;
    public sealed record Written(int Count) : ScheduleYield;
    public sealed record Profile(Seq<ScheduleProfile> Roster) : ScheduleYield;
}

// --- [ERRORS] ---------------------------------------------------------------------------
[Union]
public abstract partial record ScheduleFault : Expected, IValidationError<ScheduleFault>, Semigroup<ScheduleFault> {
    private ScheduleFault() : base() { }
    public sealed record CodecReject(string Detail) : ScheduleFault;
    public sealed record UnknownDialect(string Probe) : ScheduleFault;
    public sealed record ProjectMissing(string Title) : ScheduleFault;
    public sealed record Aggregate(Seq<ScheduleFault> Faults) : ScheduleFault;

    public override int Code => FaultBand.Schedule + Switch(
        codecReject:    static _ => 1,
        unknownDialect: static _ => 2,
        projectMissing: static _ => 3,
        aggregate:      static _ => 4);

    public override string Message => Switch(
        codecReject:    static c => $"<schedule-codec-reject:{c.Detail}>",
        unknownDialect: static c => $"<schedule-unknown-dialect:{c.Probe}>",
        projectMissing: static c => $"<schedule-project-missing:{c.Title}>",
        aggregate:      static c => $"<schedule-aggregate:{c.Faults.Count}>");

    public override string Category => Switch(
        codecReject:    static _ => "Codec",
        unknownDialect: static _ => "Dialect",
        projectMissing: static _ => "Project",
        aggregate:      static _ => "Aggregate");

    public static ScheduleFault Create(string message) => new CodecReject(message);

    public ScheduleFault Combine(ScheduleFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };

    public static ScheduleFault Lift(Exception boundary) => new CodecReject($"{boundary.GetType().Name}:{boundary.Message}");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScheduleFactKind {
    public static readonly ScheduleFactKind Parse = new("parse");
    public static readonly ScheduleFactKind Write = new("write");
    public static readonly ScheduleFactKind Probe = new("probe");
}

public readonly record struct ScheduleFact(ScheduleFactKind Kind, string Dialect, int Projects, long Activities, long Relations, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class ScheduleSource {
    public static IO<Validation<ScheduleFault, ScheduleYield>> Run(ScheduleOp op, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        op.Switch(
            (frame, sink),
            parse:     static (s, p) => Parsed(p.Spec, s.frame, s.sink),
            serialize: static (s, w) => Serialized(w.Target, w.To, w.Graph, s.frame, s.sink),
            probe:     static (s, p) => Probed(p.Spec, s.frame, s.sink));

    static IO<Validation<ScheduleFault, ScheduleYield>> Parsed(ScheduleSpec spec, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Container(spec).Bind(spec.Selected).Map(projects =>
            (ScheduleYield)new ScheduleYield.Projects(projects.Map(ProjectRows.Of))))
        from _ in rows.Match(
            Succ: y => y is ScheduleYield.Projects p
                ? sink(new ScheduleFact(ScheduleFactKind.Parse, Dialect(p.Rows), p.Rows.Count, p.Rows.Sum(static r => (long)r.Activities.Count), p.Rows.Sum(static r => (long)r.Relations.Count), frame.Now()))
                : IO.pure(unit),
            Fail: _ => IO.pure(unit))
        select rows;

    static IO<Validation<ScheduleFault, ScheduleYield>> Serialized(ScheduleSpec target, ScheduleFormat to, Seq<ScheduleProject> graph, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(() => {
            Seq<ProjectFile> files = graph.Map(Synthesis.Fold);
            target.Source.Read(
                path:   p => { Write(to, files, p); return unit; },
                stream: s => { Write(to, files, s); return unit; });
            return (ScheduleYield)new ScheduleYield.Written(files.Count);
        }))
        from _ in done.Match(
            Succ: _ => sink(new ScheduleFact(
                ScheduleFactKind.Write, to.Key, graph.Count,
                graph.Sum(static r => (long)r.Activities.Count), graph.Sum(static r => (long)r.Relations.Count), frame.Now())),
            Fail: _ => IO.pure(unit))
        select done;

    static IO<Validation<ScheduleFault, ScheduleYield>> Probed(ScheduleSpec spec, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        from roster in IO.lift(() => Container(spec).Bind(spec.Selected).Map(projects =>
            (ScheduleYield)new ScheduleYield.Profile(projects.Map(static p => new ScheduleProfile(
                p.ProjectProperties.FileType ?? "", p.ProjectProperties.FileApplication ?? "",
                p.ProjectProperties.ProjectTitle ?? "", p.Tasks.Count, p.Tasks.Sum(static t => t.Predecessors.Count))))))
        from _ in roster.Match(
            Succ: y => y is ScheduleYield.Profile profile
                ? sink(new ScheduleFact(ScheduleFactKind.Probe, "", profile.Roster.Count, 0L, 0L, frame.Now()))
                : IO.pure(unit),
            Fail: _ => IO.pure(unit))
        select roster;

    static Validation<ScheduleFault, Seq<ProjectFile>> Container(ScheduleSpec spec) =>
        Capture(() => spec.Source.Read(
            path:   p => toSeq(new UniversalProjectReader().ReadAll(p) ?? []),
            stream: s => toSeq(new UniversalProjectReader().ReadAll(s) ?? [])))
        .Bind(static projects => projects.IsEmpty
            ? (Validation<ScheduleFault, Seq<ProjectFile>>)new ScheduleFault.UnknownDialect("<no-reader-claimed-input>")
            : projects);

    static void Write(ScheduleFormat to, Seq<ProjectFile> files, string path) {
        if (files is [ProjectFile only]) { new UniversalProjectWriter(to.Wire).Write(only, path); }
        else { new UniversalProjectWriter(to.Wire).Write([.. files], path); }
    }

    static void Write(ScheduleFormat to, Seq<ProjectFile> files, Stream sink) {
        if (files is [ProjectFile only]) { new UniversalProjectWriter(to.Wire).Write(only, sink); }
        else { new UniversalProjectWriter(to.Wire).Write([.. files], sink); }
    }

    static string Dialect(Seq<ScheduleProject> rows) => rows.Head.Match(Some: static r => r.Anchor.Dialect, None: static () => "");

    internal static Validation<ScheduleFault, TValue> Capture<TValue>(Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: static value => (Validation<ScheduleFault, TValue>)value,
            Fail: static e => (Validation<ScheduleFault, TValue>)ScheduleFault.Lift(e.ToException()));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                      | [BINDING]                                                          |
| :-----: | :----------------- | :-------------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | one schedule owner | `ScheduleSource.Run` over `ScheduleOp`       | parse/serialize/probe are cases of ONE dispatch                     |
|  [02]   | container ingress  | always `ReadAll`, format-sniffed             | `Read` truncates multi-project XER; extension branching deleted     |
|  [03]   | egress axis        | `ScheduleFormat` seven writable rows         | no MPP writer exists; unreachable target unrepresentable            |
|  [04]   | egress arity       | graph count selects `Write` overload         | one vs many is the value's shape, never a flag                      |
|  [05]   | durable payload    | full activity/edge/calendar/resource rows    | the persisted P6/MS-Project record; CPM math stays `Rasm.Bim`       |
|  [06]   | unit fidelity      | `Option<ScheduleSpan>` unit-tagged crossing  | absence stays absence; re-minted via `Duration.GetInstance`         |
|  [07]   | local stamps       | `LocalDateTime`/`LocalDate` durable form     | a schedule date is unzoned wall time; no fabricated UTC `Instant`   |
|  [08]   | seam vocabularies  | three smart-enum vocabularies                | foreign enums cross once; durable rows carry keys                   |
|  [09]   | IKVM seam          | `ProjectRows.Of` / `Synthesis.Fold`          | proxy types and `JavaObject` never escape the two members           |
|  [10]   | row-boundary fault | `Validation<ScheduleFault, …>` both legs     | dialect, selector, and codec refusals stay typed                     |
|  [11]   | fault band         | `Code => FaultBand.Schedule + n`             | 8401-8404 off the `graph#FAULT_TABLES` registry                     |
|  [12]   | receipt            | one `ScheduleFact` stream `store.schedule.*` | kind-discriminated; never parallel receipt records                  |
|  [13]   | element projection | per-app schedule→element map                 | `[02]-[SEAMS]` `Ingest → Rasm.Element` wire; codec sees rows only   |

## [03]-[DURABLE_NETWORK]

- Owner: the durable-row family `[02]` declares — `ScheduleProject` the per-project aggregate, `ScheduleActivity` the WBS-threaded activity row at full axis depth, `TaskRelation` the CPM edge row, `WorkCalendarRow`/`WeekRow`/`DayRow`/`ShiftRow`/`CalendarException` the working-time record, `ResourceRow`/`AssignmentRow` the loading record, `ScheduleAnchor` the project header — plus `ProjectRows`/`Synthesis` the two IKVM seam members and `ScheduleRows` the reconciliation owner. `ScheduleChange<T>` carries one baseline/update pair, and `ScheduleVariance` partitions the aggregate diff without field-renamed slip DTOs.
- Cases: activity parentage rides `Parent`; `ScheduleDirection`, `CalendarKind`, `ResourceKind`, `ConstraintKind`, and `ScheduleUnit` preserve foreign enums without strings; activity rows retain schedule, actual, baseline, planned, remaining, budget, hierarchy, and constraint axes; calendar rows retain base weeks, overrides, recurring exceptions, and exception shifts; resource and assignment rows retain calendars, dated availability, peak units, and actual/remaining cost and work; `ScheduleVariance` carries activity, topology, lag, loading, calendar, resource, and anchor drift.
- Entry: `public static ScheduleVariance ScheduleRows.Reconcile(ScheduleProject baseline, ScheduleProject update)` — correlation prefers `ActivityId`, then WBS, then file key and applies pure set-algebra diff; everything else is values the `[02]` ops yield and accept: the store-rail write is the app's (`Element/graph#STORE_RAIL`), and `Ingest/tabular#BULK_LANE` lands these typed rows.
- Auto: round-trip fidelity is structural — `ProjectRows.Of` then `Synthesis.Fold` reconstructs the WBS-parented activity hierarchy (children minted through `Task.AddTask()` off the flat `Parent` options), every settable activity axis (name, percent, critical/milestone, schedule/actual/baseline windows, duration/work/cost, slack, constraint, WBS/activity ids), the full relation DAG with kinds and unit-tagged lags, the calendars WITH their weekly pattern, work-week overrides, and per-exception shift windows, the resources, and the loading rows, so a P6 XER ingested and re-serialized as XER preserves the network byte-meaningfully; slack and baseline rows persist as PARSED evidence of the source tool's last CPM pass and re-emit verbatim, never recomputed here; field-level fidelity beyond the durable row set (activity codes, custom fields) widens as rows on `ScheduleActivity`, never as a retained `ProjectFile`.
- Receipt: none of its own — the rows ride `[02]`'s facts; a reconciliation is a pure fold whose consumer stamps its own fact.
- Packages: covered by `[02]`.
- Growth: `ScheduleActivity`, `AssignmentRow`, and `ScheduleVariance` absorb every verified task, loading, and update-cycle axis; a durable `ProjectFile` blob, per-dialect row family, sidecar variance record, or recomputed-slack column is forbidden.
- Boundary: the rows are the Persistence half of the relocated Bim schedule domain — `Rasm.Bim/Planning/schedule` runs CPM/4D over them (read AND round-trip: an edited durable network serializes back to XER/PMXML through `[02]`'s egress so the P6 authoring tool re-imports it), and `ScheduleVariance` is what its earned-value and critical-path-drift reads consume; the app composition root maps activity rows to `Rasm.Element` nodes (row-shape law); no row carries an MPXJ type, a Java handle, an absolute re-based duration, or a fabricated UTC stamp.

```csharp signature
// --- [BOUNDARIES] -----------------------------------------------------------------------

public static class ProjectRows {
    static readonly DayOfWeek[] WeekDays =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday];

    public static ScheduleProject Of(ProjectFile file) {
        FrozenDictionary<int, int> parents = file.Tasks
            .SelectMany(static t => t.ChildTasks.Select(c => (Child: c.UniqueID ?? 0, Parent: t.UniqueID ?? 0)))
            .ToFrozenDictionary(static p => p.Child, static p => p.Parent);
        return new ScheduleProject(
            new ScheduleAnchor(
                file.ProjectProperties.FileType ?? "", file.ProjectProperties.FileApplication ?? "",
                file.ProjectProperties.ProjectTitle ?? "", ScheduleDirection.Of(file.ProjectProperties.ScheduleFrom),
                Local(file.ProjectProperties.StartDate), Local(file.ProjectProperties.FinishDate),
                Local(file.ProjectProperties.StatusDate), Local(file.ProjectProperties.CurrentDate), Optional(file.ProjectProperties.CurrencyCode),
                Optional(file.ProjectProperties.DefaultCalendarUniqueID), Optional(file.ProjectProperties.MinutesPerDay), Optional(file.ProjectProperties.DaysPerMonth)),
            toSeq(file.Tasks).Map(t => new ScheduleActivity(
                t.UniqueID ?? 0,
                parents.TryGetValue(t.UniqueID ?? 0, out int parent) ? Some(parent) : None,
                t.Name ?? "", Optional(t.WBS), Optional(t.ActivityID),
                t.PercentageComplete ?? 0d, t.Critical, t.Milestone, t.Summary, Optional(t.OutlineLevel),
                Local(t.Start), Local(t.Finish), Local(t.EarlyStart), Local(t.EarlyFinish),
                Local(t.LateStart), Local(t.LateFinish), Local(t.ActualStart), Local(t.ActualFinish),
                Local(t.BaselineStart), Local(t.BaselineFinish),
                ScheduleSpan.From(t.Duration), ScheduleSpan.From(t.Work), Optional(t.Cost), Optional(t.BudgetCost),
                ScheduleSpan.From(t.ActualDuration), ScheduleSpan.From(t.ActualWork), Optional(t.ActualCost),
                ScheduleSpan.From(t.BaselineDuration), ScheduleSpan.From(t.BaselineWork), Optional(t.BaselineCost),
                ScheduleSpan.From(t.PlannedDuration), ScheduleSpan.From(t.PlannedWork), Optional(t.PlannedCost),
                ScheduleSpan.From(t.RemainingDuration), ScheduleSpan.From(t.RemainingWork), Optional(t.RemainingCost),
                ScheduleSpan.From(t.TotalSlack), ScheduleSpan.From(t.FreeSlack),
                ConstraintKind.Of(t.ConstraintType), Local(t.ConstraintDate))),
            toSeq(file.Tasks).SelectMany(static t => t.Predecessors.Select(r => new TaskRelation(
                r.PredecessorTask?.UniqueID ?? 0, r.SuccessorTask?.UniqueID ?? 0,
                DependencyKind.Of(r.Type), ScheduleSpan.From(r.Lag)))).ToSeq(),
            toSeq(file.Calendars).Map(Calendar),
            toSeq(file.Resources).Map(static r => new ResourceRow(
                r.UniqueID ?? 0, r.Name ?? "", Optional(r.Group), ResourceKind.Of(r.Type),
                Optional(r.PeakUnits), toSeq(r.Availability).Map(a => new ResourceAvailabilityRow(
                    Local(a.Range.Start), Local(a.Range.End), Optional(a.Units))),
                Optional(r.Calendar?.UniqueID), Optional(r.Cost), Optional(r.ActualCost), Optional(r.OvertimeCost))),
            toSeq(file.ResourceAssignments).Map(static a => new AssignmentRow(
                a.Task?.UniqueID ?? 0, a.Resource?.UniqueID ?? 0, Optional(a.Units),
                ScheduleSpan.From(a.Work), Optional(a.Cost), Optional(a.BudgetCost),
                ScheduleSpan.From(a.ActualWork), Optional(a.ActualCost),
                ScheduleSpan.From(a.RemainingWork), Optional(a.RemainingCost))));
    }

    static WorkCalendarRow Calendar(ProjectCalendar calendar) => new(
        calendar.UniqueID ?? 0, calendar.Name ?? "", CalendarKind.Of(calendar.Type),
        toSeq(WeekDays).Map(day => Day(calendar, day)),
        toSeq(calendar.WorkWeeks).Map(week => new WeekRow(
            week.Name ?? "", Date(week.DateRange?.Start), Date(week.DateRange?.End),
            toSeq(WeekDays).Map(day => Day(week, day)))),
        toSeq(calendar.CalendarExceptions).Map(e => new CalendarException(
            e.Name ?? "", Date(e.FromDate), Date(e.ToDate),
            e.Recurring is { } recurrence ? Some(Recurrence(recurrence)) : None,
            Shifts(e))));

    static CalendarRecurrence Recurrence(RecurringData recurrence) => new(
        RecurrenceKind.Of(recurrence.RecurrenceType),
        Date(recurrence.StartDate),
        Date(recurrence.FinishDate),
        Optional(recurrence.Occurrences),
        recurrence.UseEndDate,
        recurrence.WorkingDaysOnly,
        recurrence.Relative,
        Optional(recurrence.Frequency),
        Optional(recurrence.DayOfWeek),
        Optional(recurrence.DayNumber),
        Optional(recurrence.MonthNumber),
        toSeq(WeekDays).Filter(recurrence.GetWeeklyDay));

    static DayRow Day(ProjectCalendarDays days, DayOfWeek day) =>
        new(day, days.GetCalendarDayType(day) == DayType.Working, Shifts(days.GetCalendarHours(day)));

    static Seq<ShiftRow> Shifts(IEnumerable<TimeOnlyRange>? ranges) => ranges is null
        ? Seq<ShiftRow>()
        : toSeq(ranges).Choose(static range => range.Start is TimeOnly start && range.End is TimeOnly end
            ? Some(new ShiftRow(start, end))
            : None);

    static Option<LocalDateTime> Local(DateTime? at) => at is { } value ? Some(LocalDateTime.FromDateTime(value)) : None;

    static Option<LocalDate> Date(DateOnly? at) => at is { } value ? Some(LocalDate.FromDateOnly(value)) : None;
}

public static class Synthesis {
    public static ProjectFile Fold(ScheduleProject project) {
        ProjectFile file = new();
        Anchor(file.ProjectProperties, project.Anchor);
        foreach (WorkCalendarRow calendar in project.Calendars) {
            ProjectCalendar made = file.AddCalendar();
            (made.UniqueID, made.Name) = (calendar.Key, calendar.Name);
            calendar.Kind.Iter(kind => made.Type = kind.Wire);
            calendar.Week.Iter(day => Pattern(made, day));
            calendar.Overrides.Iter(week => {
                ProjectCalendarWeek span = made.AddWorkWeek();
                span.Name = week.Name;
                week.From.Iter(start => span.DateRange = new DateOnlyRange(start.ToDateOnly(), week.To.IfNone(start).ToDateOnly()));
                week.Days.Iter(day => Pattern(span, day));
            });
            calendar.Exceptions.Iter(e => {
                ProjectCalendarException window = e.Recurrence.Match(
                    Some: recurrence => made.AddCalendarException(recurrence.Wire()),
                    None: () => e.From.Match(
                        Some: start => made.AddCalendarException(start.ToDateOnly(), e.To.IfNone(start).ToDateOnly()),
                        None: static () => throw new InvalidDataException("<calendar-exception-anchor>")));
                window.Name = e.Name;
                e.Shifts.Iter(shift => window.Add(new TimeOnlyRange(shift.From, shift.To)));
            });
        }
        HashMap<int, Seq<ScheduleActivity>> children = toHashMap(project.Activities
            .Choose(static a => a.Parent.Map(parent => (parent, a)))
            .GroupBy(static link => link.parent)
            .Select(static group => (group.Key, toSeq(group.Select(static link => link.a)))));
        foreach (ScheduleActivity root in project.Activities.Filter(static a => a.Parent.IsNone)) {
            Grow(file.AddTask(), root, children);
        }
        foreach (TaskRelation edge in project.Relations) {
            Relation.Builder builder = new Relation.Builder(file)
                .PredecessorTask(file.GetTaskByUniqueID(edge.Predecessor))
                .SuccessorTask(file.GetTaskByUniqueID(edge.Successor))
                .Type(edge.Kind.Wire);
            _ = edge.Lag.Match(Some: lag => builder.Lag(lag.Wire), None: () => builder).Build();
        }
        foreach (ResourceRow resource in project.Resources) {
            Resource row = file.AddResource();
            (row.UniqueID, row.Name) = (resource.Key, resource.Name);
            resource.Group.Iter(group => row.Group = group);
            resource.Kind.Iter(kind => row.Type = kind.Wire);
            resource.PeakUnits.Iter(units => row.PeakUnits = units);
            resource.Availability.Iter(span => row.Availability.Add(new Availability(
                span.From.Map(static at => at.ToDateTimeUnspecified()).ToNullable(),
                span.To.Map(static at => at.ToDateTimeUnspecified()).ToNullable(),
                span.Units.ToNullable())));
            resource.Calendar.Iter(key => row.Calendar = file.GetCalendarByUniqueID(key));
            resource.Cost.Iter(cost => row.Cost = cost);
            resource.ActualCost.Iter(cost => row.ActualCost = cost);
            resource.OvertimeCost.Iter(cost => row.OvertimeCost = cost);
        }
        foreach (AssignmentRow loading in project.Assignments) {
            ResourceAssignment made = file.GetTaskByUniqueID(loading.Activity).AddResourceAssignment(file.GetResourceByUniqueID(loading.Resource));
            loading.Units.Iter(units => made.Units = units);
            loading.Work.Iter(work => made.Work = work.Wire);
            loading.Cost.Iter(cost => made.Cost = cost);
            loading.BudgetCost.Iter(cost => made.BudgetCost = cost);
            loading.ActualWork.Iter(work => made.ActualWork = work.Wire);
            loading.ActualCost.Iter(cost => made.ActualCost = cost);
            loading.RemainingWork.Iter(work => made.RemainingWork = work.Wire);
            loading.RemainingCost.Iter(cost => made.RemainingCost = cost);
        }
        return file;
    }

    static void Anchor(ProjectProperties properties, ScheduleAnchor anchor) {
        properties.ProjectTitle = anchor.Title;
        anchor.Direction.Iter(direction => properties.ScheduleFrom = direction.Wire);
        anchor.Start.Iter(at => properties.StartDate = at.ToDateTimeUnspecified());
        anchor.Finish.Iter(at => properties.FinishDate = at.ToDateTimeUnspecified());
        anchor.Status.Iter(at => properties.StatusDate = at.ToDateTimeUnspecified());
        anchor.Current.Iter(at => properties.CurrentDate = at.ToDateTimeUnspecified());
        anchor.Currency.Iter(code => properties.CurrencyCode = code);
        anchor.DefaultCalendar.Iter(key => properties.DefaultCalendarUniqueID = key);
        anchor.MinutesPerDay.Iter(minutes => properties.MinutesPerDay = minutes);
        anchor.DaysPerMonth.Iter(days => properties.DaysPerMonth = days);
    }

    static void Pattern(ProjectCalendarDays days, DayRow day) {
        days.SetWorkingDay(day.Day, day.Working);
        if (day.Working) {
            ProjectCalendarHours hours = days.AddCalendarHours(day.Day);
            day.Shifts.Iter(shift => hours.Add(new TimeOnlyRange(shift.From, shift.To)));
        }
    }

    static void Grow(Task task, ScheduleActivity activity, HashMap<int, Seq<ScheduleActivity>> children) {
        (task.UniqueID, task.Name, task.PercentageComplete, task.Critical, task.Milestone, task.Summary) =
            (activity.Key, activity.Name, activity.Percent, activity.Critical, activity.Milestone, activity.Summary);
        activity.OutlineLevel.Iter(level => task.OutlineLevel = level);
        activity.Wbs.Iter(wbs => task.WBS = wbs);
        activity.ActivityId.Iter(id => task.ActivityID = id);
        activity.Start.Iter(at => task.Start = at.ToDateTimeUnspecified());
        activity.Finish.Iter(at => task.Finish = at.ToDateTimeUnspecified());
        activity.EarlyStart.Iter(at => task.EarlyStart = at.ToDateTimeUnspecified());
        activity.EarlyFinish.Iter(at => task.EarlyFinish = at.ToDateTimeUnspecified());
        activity.LateStart.Iter(at => task.LateStart = at.ToDateTimeUnspecified());
        activity.LateFinish.Iter(at => task.LateFinish = at.ToDateTimeUnspecified());
        activity.ActualStart.Iter(at => task.ActualStart = at.ToDateTimeUnspecified());
        activity.ActualFinish.Iter(at => task.ActualFinish = at.ToDateTimeUnspecified());
        activity.BaselineStart.Iter(at => task.BaselineStart = at.ToDateTimeUnspecified());
        activity.BaselineFinish.Iter(at => task.BaselineFinish = at.ToDateTimeUnspecified());
        activity.Duration.Iter(span => task.Duration = span.Wire);
        activity.Work.Iter(span => task.Work = span.Wire);
        activity.Cost.Iter(cost => task.Cost = cost);
        activity.BudgetCost.Iter(cost => task.BudgetCost = cost);
        activity.ActualDuration.Iter(span => task.ActualDuration = span.Wire);
        activity.ActualWork.Iter(span => task.ActualWork = span.Wire);
        activity.ActualCost.Iter(cost => task.ActualCost = cost);
        activity.BaselineDuration.Iter(span => task.BaselineDuration = span.Wire);
        activity.BaselineWork.Iter(span => task.BaselineWork = span.Wire);
        activity.BaselineCost.Iter(cost => task.BaselineCost = cost);
        activity.PlannedDuration.Iter(span => task.PlannedDuration = span.Wire);
        activity.PlannedWork.Iter(span => task.PlannedWork = span.Wire);
        activity.PlannedCost.Iter(cost => task.PlannedCost = cost);
        activity.RemainingDuration.Iter(span => task.RemainingDuration = span.Wire);
        activity.RemainingWork.Iter(span => task.RemainingWork = span.Wire);
        activity.RemainingCost.Iter(cost => task.RemainingCost = cost);
        activity.TotalSlack.Iter(span => task.TotalSlack = span.Wire);
        activity.FreeSlack.Iter(span => task.FreeSlack = span.Wire);
        activity.Constraint.Iter(kind => task.ConstraintType = kind.Wire);
        activity.ConstraintAt.Iter(at => task.ConstraintDate = at.ToDateTimeUnspecified());
        children.Find(activity.Key).IfNone(Seq<ScheduleActivity>()).Iter(child => Grow(task.AddTask(), child, children));
    }
}

// --- [OPERATIONS] -----------------------------------------------------------------------

public readonly record struct ScheduleChange<T>(T Baseline, T Update);

public sealed record ScheduleVariance(
    Seq<ScheduleActivity> ActivitiesAdded,
    Seq<ScheduleActivity> ActivitiesRemoved,
    Seq<ScheduleChange<ScheduleActivity>> ActivitiesChanged,
    Seq<TaskRelation> EdgesAdded,
    Seq<TaskRelation> EdgesRemoved,
    Seq<ScheduleChange<TaskRelation>> LagChanged,
    Seq<AssignmentRow> LoadingAdded,
    Seq<AssignmentRow> LoadingRemoved,
    Seq<ScheduleChange<AssignmentRow>> LoadingChanged,
    Seq<WorkCalendarRow> CalendarsAdded,
    Seq<WorkCalendarRow> CalendarsRemoved,
    Seq<ScheduleChange<WorkCalendarRow>> CalendarsChanged,
    Seq<ResourceRow> ResourcesAdded,
    Seq<ResourceRow> ResourcesRemoved,
    Seq<ScheduleChange<ResourceRow>> ResourcesChanged,
    Option<ScheduleChange<ScheduleAnchor>> AnchorChanged);

public static class ScheduleRows {
    public static ScheduleVariance Reconcile(ScheduleProject baseline, ScheduleProject update) {
        HashMap<string, ScheduleActivity> held = toHashMap(baseline.Activities.Map(static activity => (activity.Correlation, activity)));
        HashMap<string, ScheduleActivity> fresh = toHashMap(update.Activities.Map(static activity => (activity.Correlation, activity)));
        HashMap<int, string> heldIds = toHashMap(baseline.Activities.Map(static activity => (activity.Key, activity.Correlation)));
        HashMap<int, string> freshIds = toHashMap(update.Activities.Map(static activity => (activity.Key, activity.Correlation)));
        HashMap<(string, string, string), TaskRelation> heldLogic = toHashMap(baseline.Relations.Map(edge => (Logic(edge, heldIds), edge)));
        HashMap<(string, string, string), TaskRelation> freshLogic = toHashMap(update.Relations.Map(edge => (Logic(edge, freshIds), edge)));
        HashMap<(string, int), AssignmentRow> heldLoading = toHashMap(baseline.Assignments.Map(row => (Loading(row, heldIds), row)));
        HashMap<(string, int), AssignmentRow> freshLoading = toHashMap(update.Assignments.Map(row => (Loading(row, freshIds), row)));
        HashMap<int, WorkCalendarRow> heldCalendars = toHashMap(baseline.Calendars.Map(static row => (row.Key, row)));
        HashMap<int, WorkCalendarRow> freshCalendars = toHashMap(update.Calendars.Map(static row => (row.Key, row)));
        HashMap<int, ResourceRow> heldResources = toHashMap(baseline.Resources.Map(static row => (row.Key, row)));
        HashMap<int, ResourceRow> freshResources = toHashMap(update.Resources.Map(static row => (row.Key, row)));
        return new ScheduleVariance(
            ActivitiesAdded: update.Activities.Filter(activity => !held.ContainsKey(activity.Correlation)),
            ActivitiesRemoved: baseline.Activities.Filter(activity => !fresh.ContainsKey(activity.Correlation)),
            ActivitiesChanged: update.Activities.Choose(activity => held.Find(activity.Correlation)
                .Filter(prior => prior != activity)
                .Map(prior => new ScheduleChange<ScheduleActivity>(prior, activity))),
            EdgesAdded: update.Relations.Filter(edge => !heldLogic.ContainsKey(Logic(edge, freshIds))),
            EdgesRemoved: baseline.Relations.Filter(edge => !freshLogic.ContainsKey(Logic(edge, heldIds))),
            LagChanged: update.Relations.Choose(edge => heldLogic.Find(Logic(edge, freshIds))
                .Filter(prior => prior.Lag != edge.Lag)
                .Map(prior => new ScheduleChange<TaskRelation>(prior, edge))),
            LoadingAdded: update.Assignments.Filter(row => !heldLoading.ContainsKey(Loading(row, freshIds))),
            LoadingRemoved: baseline.Assignments.Filter(row => !freshLoading.ContainsKey(Loading(row, heldIds))),
            LoadingChanged: update.Assignments.Choose(row => heldLoading.Find(Loading(row, freshIds))
                .Filter(prior => prior != row)
                .Map(prior => new ScheduleChange<AssignmentRow>(prior, row))),
            CalendarsAdded: update.Calendars.Filter(row => !heldCalendars.ContainsKey(row.Key)),
            CalendarsRemoved: baseline.Calendars.Filter(row => !freshCalendars.ContainsKey(row.Key)),
            CalendarsChanged: update.Calendars.Choose(row => heldCalendars.Find(row.Key)
                .Filter(prior => prior != row)
                .Map(prior => new ScheduleChange<WorkCalendarRow>(prior, row))),
            ResourcesAdded: update.Resources.Filter(row => !heldResources.ContainsKey(row.Key)),
            ResourcesRemoved: baseline.Resources.Filter(row => !freshResources.ContainsKey(row.Key)),
            ResourcesChanged: update.Resources.Choose(row => heldResources.Find(row.Key)
                .Filter(prior => prior != row)
                .Map(prior => new ScheduleChange<ResourceRow>(prior, row))),
            AnchorChanged: baseline.Anchor == update.Anchor
                ? None
                : Some(new ScheduleChange<ScheduleAnchor>(baseline.Anchor, update.Anchor)));
    }

    static (string, string, string) Logic(TaskRelation edge, HashMap<int, string> identities) => (
        identities.Find(edge.Predecessor).IfNone($"key:{edge.Predecessor}"),
        identities.Find(edge.Successor).IfNone($"key:{edge.Successor}"),
        edge.Kind.Key);

    static (string, int) Loading(AssignmentRow row, HashMap<int, string> identities) => (
        identities.Find(row.Activity).IfNone($"key:{row.Activity}"),
        row.Resource);
}
```

| [INDEX] | [POLICY]       | [VALUE]                                     | [BINDING]                                                       |
| :-----: | :------------- | :------------------------------------------ | :--------------------------------------------------------------- |
|  [01]   | WBS shape      | flat rows + `Parent` option                 | depth-free, bulk-lane-shaped; never a recursive durable tree     |
|  [02]   | edge dedup     | predecessor-side read only                  | MPXJ's symmetric pair lands once                                 |
|  [03]   | slack/baseline | parsed evidence, re-emitted verbatim        | the source tool's CPM pass is evidence, not authority            |
|  [04]   | calendar depth | weekly pattern + overrides + own shifts     | no default-shift narrowing; exceptions carry their hour windows  |
|  [05]   | 5D crossing    | raw doubles on `AssignmentRow`              | `Money`/`UnitsNet` lift at the Bim boundary, never here          |
|  [06]   | round-trip     | `Of` then `Fold` preserves the network      | XER out re-imports in P6; every settable axis re-emits           |
|  [07]   | update cycle   | `Reconcile` → `ScheduleVariance`             | diffs every durable aggregate axis                             |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
