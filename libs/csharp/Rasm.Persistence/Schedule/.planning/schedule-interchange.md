# [PERSISTENCE_SCHEDULE_INTERCHANGE]

Rasm.Persistence owns the construction-schedule store and its 4D coupling: `ScheduleImport` reads a Primavera P6 XER (tab-delimited) and a Microsoft Project XML into a typed `ScheduleTask` activity network with predecessor/successor relationships and WBS hierarchy; `TaskElementLink` binds a schedule activity to a federated `ElementSet` so a 4D state derives which elements are planned, in-progress, or complete at any date; and `FourDState` folds the activity network plus its element links plus an as-of date into the per-element 4D status the viewport and the time-travel engine consume. The Sep tabular reader (`data-lanes#ANALYTICAL_LANE`), the cross-document link (`federation#CROSS_DOC_LINKS`), the element-set currency (`federation#ELEMENT_SET_ALGEBRA`), the time-travel AS-OF fold (`version-control#TIME_TRAVEL`), and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled. The Compute P6/XER parse companion produces the canonical activity bytes this store ingests.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                             |
| :-----: | :--------------- | :---------------------------------------------------------------- |
|   [1]   | SCHEDULE_STORE   | P6 XER + MS-Project XML import; typed activity network; WBS        |
|   [2]   | TASK_LINK_4D     | Task-to-element-set link; 4D state fold; as-of construction status |
|   [3]   | TS_PROJECTION    | Schedule task, link, 4D-state wire shapes                         |

## [2]-[SCHEDULE_STORE]

- Owner: `ScheduleFormat` the interchange-format axis (P6 XER, MS-Project XML); `ScheduleTask` the typed activity record; `TaskRelation` the predecessor/successor dependency; `ScheduleImport` the static surface owning the XER tab-delimited read, the MS-Project XML read, the activity-network projection, and the WBS hierarchy fold.
- Cases: `P6Xer | MsProjectXml` on `ScheduleFormat`; a task carries id, WBS path, name, planned/actual start and finish, duration, and percent-complete; a relation is `FinishToStart | StartToStart | FinishToFinish | StartToFinish` with a lag.
- Entry: `public static Fin<Seq<ScheduleTask>> ReadXer(TabularExportSpec spec, Stream xer)` — reads the P6 XER `TASK`/`TASKPRED`/`PROJWBS` tables through the Sep tabular reader into typed tasks, aborting on a malformed table header; `public static Fin<Seq<ScheduleTask>> ReadMsProject(ReadOnlyMemory<byte> xml)` reads the MS-Project XML `<Task>`/`<PredecessorLink>` elements through STJ source generation.
- Auto: the P6 XER is a tab-delimited multi-table text format so it rides the Sep tabular reader (`data-lanes#ANALYTICAL_LANE`) with the tab separator and the per-table header rows — the `TASK` table projects activities, `TASKPRED` projects relations, and `PROJWBS` projects the WBS hierarchy as an ltree path, so no per-format schedule library enters; the MS-Project XML is read through STJ source generation over the `Project`/`Tasks`/`Task` element shape so it shares one `ScheduleTask` projection; the activity network is a DAG keyed on task id with `TaskRelation` edges so a critical-path or float computation rides the same DAG-walk the commit-DAG and lineage-DAG use; the WBS hierarchy is an ltree path so a roll-up-by-WBS rides the ltree operators; the imported schedule is content-addressed so a re-imported revision dedupes and a baseline-versus-current is a content-addressed diff.
- Receipt: an import rides `store.schedule.import` carrying the format, the task count, and the relation count; a WBS fold rides `store.schedule.wbs`.
- Packages: Sep, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new interchange format is one `ScheduleFormat` row plus its reader; a new task field is one column on `ScheduleTask`; a new relation type is one `RelationKind` row; zero new surface — a per-format schedule model, a Primavera SDK wrapper, or an MS-Project COM interop is the deleted form because the XER rides the Sep tabular reader, the XML rides STJ, and both project the one `ScheduleTask` activity network.
- Boundary: the P6 XER rides the Sep tabular reader so the import is a tabular read, never a Primavera SDK or a per-vendor schedule library — the XER is a tab-separated multi-table file whose `%T`/`%F`/`%R` table markers delimit `TASK`/`TASKPRED`/`PROJWBS` sections, and the Sep reader's header-named column projection (`data-lanes#ANALYTICAL_LANE` `Cols.Select`) reads the activity columns by name so a positional ordinal parse is the deleted form; the MS-Project XML rides STJ source generation over the documented schema so a COM interop or an Office library is the deleted form; both formats project the one `ScheduleTask` activity network so a per-format model is the deleted form; the activity network is a DAG so critical-path, total-float, and free-float computations are DAG walks over the relation edges, never a per-metric calculator; dates ride NodaTime `LocalDate`/`LocalDateTime` so a schedule date never becomes a `DateTime` sentinel; the WBS is an ltree path so a cost-by-WBS rollup (`catalog-cost#COST_ROLLUP`) joins the schedule WBS to the takeoff classification through one ltree join; the schedule is content-addressed so a baseline schedule and a current schedule are two content keys and a schedule variance is their diff.

```csharp signature
public sealed class ScheduleKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
[KeyMemberComparer<ScheduleKeyPolicy, string>]
public sealed partial class ScheduleFormat {
    public static readonly ScheduleFormat P6Xer = new("p6-xer", separator: '\t');
    public static readonly ScheduleFormat MsProjectXml = new("ms-project-xml", separator: '\0');

    public char Separator { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class RelationKind {
    public static readonly RelationKind FinishToStart = new("FS");
    public static readonly RelationKind StartToStart = new("SS");
    public static readonly RelationKind FinishToFinish = new("FF");
    public static readonly RelationKind StartToFinish = new("SF");
}

public sealed record TaskRelation(string Predecessor, string Successor, RelationKind Kind, Duration Lag);

public sealed record ScheduleTask(
    string TaskId,
    string WbsPath,
    string Name,
    LocalDate PlannedStart,
    LocalDate PlannedFinish,
    Option<LocalDate> ActualStart,
    Option<LocalDate> ActualFinish,
    Duration PlannedDuration,
    double PercentComplete,
    Seq<TaskRelation> Predecessors,
    UInt128 ScheduleEdition);

public static class ScheduleImport {
    public static Fin<Seq<ScheduleTask>> ReadXer(TabularExportSpec spec, Stream xer, Func<SepReader, RowKind, Seq<ScheduleTask>> projectTasks) =>
        spec.Direction == TabularDirection.Import
            ? spec.OpenReader(xer).Map(reader => projectTasks(reader, RowKind.Task))
            : Fin.Fail<Seq<ScheduleTask>>(Error.New("<schedule-import-requires-import-direction>"));

    public static Fin<Seq<ScheduleTask>> ReadMsProject(ReadOnlyMemory<byte> xml, Func<ReadOnlyMemory<byte>, Fin<Seq<ScheduleTask>>> parseXml) =>
        parseXml(xml);

    public static Seq<string> CriticalPath(Seq<ScheduleTask> network) {
        var byId = network.ToHashMap(static task => task.TaskId);
        var longest = network.Fold(HashMap<string, Duration>(), (acc, task) =>
            acc.Add(task.TaskId, LongestTo(byId, acc, task)));
        return toSeq(longest.OrderByDescending(static slot => slot.Value).Map(static slot => slot.Key).Take(1));
    }

    private static Duration LongestTo(HashMap<string, ScheduleTask> byId, HashMap<string, Duration> acc, ScheduleTask task) =>
        task.Predecessors.IsEmpty
            ? task.PlannedDuration
            : task.Predecessors.Map(rel => acc.Find(rel.Predecessor).IfNone(Duration.Zero) + rel.Lag + task.PlannedDuration).Max();
}

[SmartEnum]
public sealed partial class RowKind {
    public static readonly RowKind Task = new();
    public static readonly RowKind Relation = new();
    public static readonly RowKind Wbs = new();
}
```

| [INDEX] | [FORMAT]         | [SURFACE]                          | [TABLES]                                          |
| :-----: | :--------------- | :--------------------------------- | :------------------------------------------------ |
|   [1]   | P6 XER           | Sep tab-delimited multi-table read | `TASK` activities, `TASKPRED` relations, `PROJWBS` WBS |
|   [2]   | MS-Project XML   | STJ source-generated read          | `Task` elements, `PredecessorLink` relations      |

## [3]-[TASK_LINK_4D]

- Owner: `TaskElementLink` the schedule-activity-to-element-set binding; `FourDStatus` the per-element construction-state axis; `FourDState` the static surface owning the task-element link, the as-of 4D state fold, and the planned-versus-actual variance projection.
- Cases: a link binds a `ScheduleTask` to an `ElementSet` (the elements that activity constructs); `NotStarted | InProgress | Complete | Demolished` on `FourDStatus`; the 4D state folds the activity network plus its links plus an as-of date into the per-element status.
- Entry: `public static TaskElementLink Link(string taskId, ElementSet elements, FourDStatus completeStatus)` — binds an activity to the elements it constructs; `public static HashMap<UInt128, FourDStatus> StateAt(Seq<ScheduleTask> network, Seq<TaskElementLink> links, LocalDate asOf)` folds the per-element 4D status at a date.
- Auto: a task-element link is a `CrossDocLink` of kind `Aggregation` (`federation#CROSS_DOC_LINKS`) from the schedule activity to the element-set, so the 4D binding rides the federated link graph and a schedule change propagates impact to the linked elements; the 4D state at a date folds each activity's planned/actual dates against the as-of date — an element whose activity is complete is `Complete`, in its date window is `InProgress`, before its start is `NotStarted`, and a demolition activity flips it to `Demolished` — so a viewport scrub over dates rides the same AS-OF fold the time-travel engine uses; planned-versus-actual variance compares the planned 4D state to the actual 4D state at a date so a schedule slip surfaces as the element-set whose actual status lags planned; the 4D state is an `ElementSet`-keyed map so it composes with the element-set algebra and feeds the viewport overlay through the same `SpatialDelta`-class binding.
- Receipt: a link rides `store.schedule.link`; a 4D state fold rides `store.schedule.4d` carrying the as-of date and the per-status element counts; a variance rides `store.schedule.variance`.
- Packages: System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new construction state is one `FourDStatus` row; a new link semantic is one `CrossDocLink.Kind` value (owned at federation); a new variance dimension is one fold projection; zero new surface — a per-element schedule column, a 4D-specific link table, or a separate construction-sequence model is the deleted form because the link is one cross-doc link, the 4D state is one date-fold over the activity network, and the result is an element-set-keyed map.
- Boundary: the task-element link is a `CrossDocLink` so the 4D binding rides the federated link graph — a 4D-specific link table is the deleted form, and a schedule change propagates to the linked elements through the same transitive impact analysis; the 4D state is a pure fold of the activity network plus its links plus an as-of date so a viewport scrub over construction dates is reproducible and rides the same AS-OF fold the time-travel engine uses — a per-date stored 4D snapshot is the deleted form; planned-versus-actual variance is the difference of two 4D folds (the planned-date fold and the actual-date fold) so a schedule slip is an element-set delta, never a per-element variance column; the 4D status feeds the viewport overlay through the element-set-keyed map so the construction sequence animates through the same overlay binding the spatial-diff stream uses (`AppUi/charts-dashboards#SERIES_TABLE`); the schedule dates ride NodaTime `LocalDate` so an as-of fold is calendar-correct and a `DateTime` sentinel never enters; the cost rollup (`catalog-cost#COST_ROLLUP`) joins the 4D state so a cash-flow-over-time (the cost of the elements complete at each date) is the cost rollup over the as-of `Complete` element-set, making 4D the bridge to 5D.

```csharp signature
[SmartEnum]
public sealed partial class FourDStatus {
    public static readonly FourDStatus NotStarted = new();
    public static readonly FourDStatus InProgress = new();
    public static readonly FourDStatus Complete = new();
    public static readonly FourDStatus Demolished = new();
}

public sealed record TaskElementLink(string TaskId, ElementSet Elements, FourDStatus CompleteStatus, Instant At);

public static class FourDState {
    public static TaskElementLink Link(string taskId, ElementSet elements, FourDStatus completeStatus, ClockPolicy clocks) =>
        new(taskId, elements, completeStatus, clocks.Now);

    public static HashMap<UInt128, FourDStatus> StateAt(Seq<ScheduleTask> network, Seq<TaskElementLink> links, LocalDate asOf) {
        var byId = network.ToHashMap(static task => task.TaskId);
        return links.Fold(HashMap<UInt128, FourDStatus>(), (acc, link) =>
            byId.Find(link.TaskId).Match(
                Some: task => link.Elements.Keys.Fold(acc, (inner, key) =>
                    inner.AddOrUpdate(key, _ => StatusOf(task, link, asOf), StatusOf(task, link, asOf))),
                None: () => acc));
    }

    public static (ElementSet Ahead, ElementSet Behind) Variance(Seq<ScheduleTask> network, Seq<TaskElementLink> links, LocalDate asOf) {
        var planned = StateAt(network, links, asOf);
        var actual = ActualStateAt(network, links, asOf);
        var ahead = planned.Filter((key, status) => actual.Find(key).Map(a => Rank(a) > Rank(status)).IfNone(false));
        var behind = planned.Filter((key, status) => actual.Find(key).Map(a => Rank(a) < Rank(status)).IfNone(false));
        return (ElementSet.Of(toSeq(ahead.Keys)), ElementSet.Of(toSeq(behind.Keys)));
    }

    private static FourDStatus StatusOf(ScheduleTask task, TaskElementLink link, LocalDate asOf) =>
        asOf < task.PlannedStart ? FourDStatus.NotStarted
        : asOf >= task.PlannedFinish ? link.CompleteStatus
        : FourDStatus.InProgress;

    private static FourDStatus ActualStatusOf(ScheduleTask task, TaskElementLink link, LocalDate asOf) =>
        task.ActualStart.Match(
            Some: start => asOf < start ? FourDStatus.NotStarted
                : task.ActualFinish.Map(finish => asOf >= finish ? link.CompleteStatus : FourDStatus.InProgress).IfNone(FourDStatus.InProgress),
            None: () => FourDStatus.NotStarted);

    private static HashMap<UInt128, FourDStatus> ActualStateAt(Seq<ScheduleTask> network, Seq<TaskElementLink> links, LocalDate asOf) {
        var byId = network.ToHashMap(static task => task.TaskId);
        return links.Fold(HashMap<UInt128, FourDStatus>(), (acc, link) =>
            byId.Find(link.TaskId).Match(
                Some: task => link.Elements.Keys.Fold(acc, (inner, key) =>
                    inner.AddOrUpdate(key, _ => ActualStatusOf(task, link, asOf), ActualStatusOf(task, link, asOf))),
                None: () => acc));
    }

    private static int Rank(FourDStatus status) =>
        status.Switch(notStarted: static () => 0, inProgress: static () => 1, complete: static () => 2, demolished: static () => 3);
}
```

| [INDEX] | [CONCERN]          | [SURFACE]                                       | [LAW]                                             |
| :-----: | :----------------- | :---------------------------------------------- | :------------------------------------------------ |
|   [1]   | task-element link  | `CrossDocLink` of kind `Aggregation`            | rides the federated link graph; impact propagates |
|   [2]   | 4D state           | as-of date-fold over the activity network       | reproducible; rides the time-travel AS-OF fold    |
|   [3]   | planned vs actual  | difference of planned-fold and actual-fold      | schedule slip is an element-set delta             |
|   [4]   | 4D-to-5D bridge    | cost rollup over the as-of `Complete` set       | cash-flow-over-time is the 5D cost of 4D state    |

## [4]-[TS_PROJECTION]

- Owner: `ScheduleFormatKind`, `ScheduleTaskWire`, `TaskRelationWire`, `RelationKindWire`, `TaskElementLinkWire`, `FourDStatusKind`, `FourDStateWire` — the schedule wire surface the TS-web Gantt and 4D scrubber decode.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: dates cross as ISO-8601 date strings (NodaTime `LocalDate` round-trip); durations cross as the roundtrip-pattern string; the relation kind crosses as the two-letter code and reconstructs as the literal union; the 4D status crosses as the case name; the element-set crosses as the content-key string array; the WBS path crosses as the ltree path string so the Gantt renders the hierarchy; the 4D state map crosses as a content-key-to-status record so the scrubber paints the viewport at an as-of date.

```ts contract
type ScheduleFormatKind = "p6-xer" | "ms-project-xml";

type RelationKindWire = "FS" | "SS" | "FF" | "SF";

type FourDStatusKind = "NotStarted" | "InProgress" | "Complete" | "Demolished";

interface TaskRelationWire {
  predecessor: string;
  successor: string;
  kind: RelationKindWire;
  lag: string;
}

interface ScheduleTaskWire {
  taskId: string;
  wbsPath: string;
  name: string;
  plannedStart: string;
  plannedFinish: string;
  actualStart: string | null;
  actualFinish: string | null;
  plannedDuration: string;
  percentComplete: number;
  predecessors: TaskRelationWire[];
}

interface TaskElementLinkWire {
  taskId: string;
  elements: string[];
  completeStatus: FourDStatusKind;
  at: string;
}

interface FourDStateWire {
  asOf: string;
  state: Record<string, FourDStatusKind>;
}
```

## [5]-[RESEARCH]

- [XER_TABLE_GRAMMAR]: the P6 XER `%T`/`%F`/`%R`/`%E` table-marker grammar and the `TASK`/`TASKPRED`/`PROJWBS` column names the Sep header-named projection reads — the activity-id, planned/actual date, duration, and percent-complete columns and the `TASKPRED` predecessor/successor/lag columns, verified against a real XER export before the reader fence pins the column projection.
- [MSPROJECT_XML_SHAPE]: the MS-Project XML `Project`/`Tasks`/`Task`/`PredecessorLink` element shape and date encoding the STJ source-generated reader transcribes, verified against a real `.xml` export, and the Compute P6/XER parse companion's canonical-activity-bytes hand-off boundary.
