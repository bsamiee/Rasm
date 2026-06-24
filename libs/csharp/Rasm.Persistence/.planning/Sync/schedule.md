# [PERSISTENCE_SCHEDULE_INTERCHANGE]

Rasm.Persistence owns the durable external-scheduler store and its sync — the Primavera P6 XER and Microsoft Project XML residence plus its CPM float algebra over the external scheduler's relation DAG. `ScheduleImport` reads a P6 XER (tab-delimited) and an MS-Project XML into a typed `ScheduleTask` activity network with predecessor/successor relationships and WBS hierarchy; `CpmPass` runs the forward/backward float pass — early/late start-finish and total/free float over the external scheduler's `TaskRelation` DAG — so the critical path is the total-float-zero activity set, a schedule slip is a typed `ScheduleFloat` erosion delta, and `ScheduleBaseline` is one content-addressed snapshot so a baseline-versus-current variance is a multi-baseline content-key diff; the CPM walk is one DAG algebra serving CPM, lineage, and the commit-DAG. The 4D construction-state domain semantics — the schedule-activity-to-`ElementSet` binding, the per-element `FourDStatus`, the as-of construction-state fold, and the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — are the AEC-domain owner `Rasm.Bim/schedule`'s concern, consumed here only at the `Sync/schedule ⇄ Rasm.Bim/schedule # [WIRE]: P6/MS-Project + 4D construction domain` wire; per the `[SCHEDULE_NETWORK_DEPTH]` boundary the source lanes split — Persistence owns the durable P6/MS-Project store plus its `CpmPass` over the external `TaskRelation` DAG, Bim owns the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — strata-legal as app-platform consuming aec-domain. The two stores federate by the IFC `GlobalId`: a `ScheduleTask` carries the `Option<string> GlobalId` correlation to the Bim `ConstructionTask.GlobalId` (the IFC process the external P6/MS-Project activity maps to, `None` for an activity with no IFC counterpart), and the wire join keys the durable external float overlay to the host-neutral CPM/4D domain by that `GlobalId` — the external `TaskId` is the P6/MS-Project activity code, never the federation key, so a Bim `CriticalPath` reading a P6 relation or a Persistence `CpmPass` re-deriving the IFC `SequenceRel` network is the cross-package drift the relocation forbids. The Sep tabular reader (`Query/lanes#ANALYTICAL_LANE`), the cross-document link (`Query/federation#CROSS_DOC_LINKS`), the element-set currency (`Query/federation#ELEMENT_SET_ALGEBRA`), the time-travel AS-OF fold (`Version/timetravel#TIME_TRAVEL`), and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled. The Compute P6/XER parse companion produces the canonical activity bytes this store ingests.

## [01]-[INDEX]

- [01]-[SCHEDULE_STORE]: P6 XER and MS-Project XML import; typed activity network; WBS; CPM float algebra.
- [02]-[TS_PROJECTION]: schedule task, relation, float, and baseline wire shapes.

## [02]-[SCHEDULE_STORE]

- Owner: `ScheduleFormat` the interchange-format axis (P6 XER, MS-Project XML); `ScheduleTask` the typed activity record carrying the resource-loading columns; `TaskRelation` the predecessor/successor dependency; `ScheduleFloat` the typed per-activity early/late start-finish + total/free float delta; `ScheduleBaseline` the content-addressed baseline-snapshot axis; `ScheduleImport` the static surface owning the XER tab-delimited read, the MS-Project XML read, the activity-network projection, the WBS hierarchy fold, and the critical-path projection over `CpmPass`; `CpmPass` the static forward/backward DAG-walk fold owning the float pass.
- Cases: `P6Xer | MsProjectXml` on `ScheduleFormat`; a task carries id, WBS path, name, planned/actual start and finish, duration, percent-complete, and the resource-id/units/budgeted-cost loading columns; a relation is `FinishToStart | StartToStart | FinishToFinish | StartToFinish` with a lag; `ScheduleFloat` carries the early/late start-finish and total/free float, the critical activity being the total-float-zero set; `ScheduleBaseline` is a content-keyed snapshot of the activity-edition set.
- Entry: `public static Fin<Seq<ScheduleTask>> ReadXer(TabularExportSpec spec, Stream xer)` — reads the P6 XER `TASK`/`TASKPRED`/`PROJWBS` tables through the Sep tabular reader into typed tasks, aborting on a malformed table header; `public static Fin<Seq<ScheduleTask>> ReadMsProject(ReadOnlyMemory<byte> xml)` reads the MS-Project XML `<Task>`/`<PredecessorLink>` elements through STJ source generation; `public static HashMap<string, ScheduleFloat> CpmPass.Pass(Seq<ScheduleTask> network)` runs the forward/backward float pass; `public static Seq<string> CriticalPath(Seq<ScheduleTask> network)` projects the total-float-zero activity set; `public static ScheduleBaseline ScheduleBaseline.Of(string name, Seq<ScheduleTask> network, Instant at)` mints the content-addressed baseline.
- Auto: the P6 XER is a tab-delimited multi-table text format so it rides the Sep tabular reader (`Query/lanes#ANALYTICAL_LANE`) with the tab separator and the per-table header rows — the `TASK` table projects activities, `TASKPRED` projects relations, and `PROJWBS` projects the WBS hierarchy as an ltree path, so no per-format schedule library enters; the MS-Project XML is read through STJ source generation over the `Project`/`Tasks`/`Task` element shape so it shares one `ScheduleTask` projection; the activity network is a DAG keyed on task id with `TaskRelation` edges so the CPM float pass is one DAG-walk the commit-DAG and lineage-DAG share — `CpmPass.Pass` topologically orders the DAG, the forward pass computes early start/finish dispatching the four `RelationKind` arms (FS/SS/FF/SF) with lag through the existing `[SmartEnum]` generated total `Switch`, the backward pass computes late start/finish over the reverse order, and total float is `Period.Between(ES, LS)` while free float is the minimum successor early-start gap; the critical path is the total-float-zero activity set rather than the single longest task; the WBS hierarchy is an ltree path so a roll-up-by-WBS rides the ltree operators; the resource-loading columns let a resource-leveled schedule and an over-allocation surface as activity-network folds over the `ResourceId`/`ResourceUnits` columns; the imported schedule is content-addressed so a re-imported revision dedupes and a `ScheduleBaseline.Of` snapshot is one content key, a baseline-versus-current variance being a multi-baseline content-key diff rather than a per-baseline table.
- Receipt: an import rides `store.schedule.import` carrying the format, the task count, and the relation count; a WBS fold rides `store.schedule.wbs`.
- Packages: Sep, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new interchange format is one `ScheduleFormat` row plus its reader; a new task field is one column on `ScheduleTask` (the resource-loading columns are exactly this — columns on the existing record, never a second resource record); a new relation type is one `RelationKind` row breaking every `CpmPass` `Switch` site at compile time; a new float metric is one column on `ScheduleFloat`; a new baseline is one `ScheduleBaseline.Of` content key, never a per-baseline table; zero new surface — a per-format schedule model, a Primavera SDK wrapper, an MS-Project COM interop, or a per-metric critical-path calculator is the deleted form because the XER rides the Sep tabular reader, the XML rides STJ, both project the one `ScheduleTask` activity network, and CPM is one DAG algebra serving CPM, lineage, and the commit-DAG.
- Boundary: the P6 XER rides the Sep tabular reader so the import is a tabular read, never a Primavera SDK or a per-vendor schedule library — the XER is a tab-separated multi-table file whose `%T`/`%F`/`%R` table markers delimit `TASK`/`TASKPRED`/`PROJWBS` sections, and the Sep reader's header-named column projection (`Query/lanes#ANALYTICAL_LANE` `Cols.Select`) reads the activity columns by name so a positional ordinal parse is the deleted form; the MS-Project XML rides STJ source generation over the documented schema so a COM interop or an Office library is the deleted form; both formats project the one `ScheduleTask` activity network so a per-format model is the deleted form; the activity network is a DAG so the critical-path, total-float, and free-float computations are one `CpmPass` forward/backward DAG walk over the relation edges — the float pass dispatches the four `RelationKind` arms through the existing `[SmartEnum]` generated total `Switch` with lag (a statement-switch or if/else ladder over the four arms is the deleted form), and the critical path is the total-float-zero activity set, never the single longest-duration task the prior stub returned; `ScheduleFloat` is a typed record struct carrying the float-erosion delta a schedule slip becomes, never a loose tuple; the resource-loading columns (`ResourceId`/`ResourceUnits`/`BudgetedCost`) ride the existing record so a resource-leveled fold and an over-allocation surface are activity-network folds, never a second resource record; `ScheduleBaseline` rides the existing content-addressed snapshot identity (`XxHash128` over the sorted activity-edition set) so a baseline-versus-current variance is a multi-baseline content-key diff and a per-baseline table is the deleted form; retained/progressed-logic scheduling reads the existing `ActualStart`/`ActualFinish` `Option<LocalDate>` against the data date with no new field; dates ride NodaTime `LocalDate`/`Period`/`Duration` so a schedule date never becomes a `DateTime` sentinel; the WBS is an ltree path so a cost-by-WBS rollup (`Store/profiles#COST_ROLLUP`) joins the schedule WBS to the takeoff classification through one ltree join; the Compute P6/XER parse companion (`Rasm.Compute/Runtime/codecs#FIELD_RESULT_CODEC` `InterchangeIo` tabular ingest, the same `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam the bulk pipeline reads) produces the canonical activity bytes this store ingests — Compute owns the parse, Persistence owns the durable residence and the CPM float algebra.

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
    Option<string> GlobalId,
    string WbsPath,
    string Name,
    LocalDate PlannedStart,
    LocalDate PlannedFinish,
    Option<LocalDate> ActualStart,
    Option<LocalDate> ActualFinish,
    Duration PlannedDuration,
    double PercentComplete,
    Seq<TaskRelation> Predecessors,
    string ResourceId,
    double ResourceUnits,
    double BudgetedCost,
    UInt128 ScheduleEdition);

public readonly record struct ScheduleFloat(
    string TaskId,
    LocalDate EarlyStart,
    LocalDate EarlyFinish,
    LocalDate LateStart,
    LocalDate LateFinish,
    Duration TotalFloat,
    Duration FreeFloat) {
    public bool Critical => TotalFloat == Duration.Zero;
}

public readonly record struct ScheduleBaseline(string Name, UInt128 Edition, Instant At) : IComparable<ScheduleBaseline> {
    public static ScheduleBaseline Of(string name, Seq<ScheduleTask> network, Instant at) {
        var buffer = new ArrayBufferWriter<byte>();
        foreach (var task in network.OrderBy(static t => t.TaskId, StringComparer.Ordinal))
            BinaryPrimitives.WriteUInt128LittleEndian(buffer.GetSpan(16)[..16], task.ScheduleEdition);
        buffer.Advance(network.Count * 16);
        return new ScheduleBaseline(name, XxHash128.HashToUInt128(buffer.WrittenSpan), at);
    }

    public int CompareTo(ScheduleBaseline other) => At.CompareTo(other.At);
}

public static class ScheduleImport {
    public static Fin<Seq<ScheduleTask>> ReadXer(TabularExportSpec spec, Stream xer, Func<SepReader, RowKind, Seq<ScheduleTask>> projectTasks) =>
        spec.Direction == TabularDirection.Import
            ? spec.OpenReader(xer).Map(reader => projectTasks(reader, RowKind.Task))
            : Fin.Fail<Seq<ScheduleTask>>(Error.New("<schedule-import-requires-import-direction>"));

    public static Fin<Seq<ScheduleTask>> ReadMsProject(ReadOnlyMemory<byte> xml, Func<ReadOnlyMemory<byte>, Fin<Seq<ScheduleTask>>> parseXml) =>
        parseXml(xml);

    public static Seq<string> CriticalPath(Seq<ScheduleTask> network) =>
        toSeq(CpmPass.Pass(network).Values.Filter(static slot => slot.Critical).Map(static slot => slot.TaskId));
}

public static class CpmPass {
    public static HashMap<string, ScheduleFloat> Pass(Seq<ScheduleTask> network) {
        var byId = network.ToHashMap(static task => task.TaskId);
        var order = TopoOrder(network, byId);
        var early = Forward(order, byId);
        var late = Backward(toSeq(order.Reverse()), byId, early);
        return order.Fold(HashMap<string, ScheduleFloat>(), (acc, id) => byId.Find(id).Match(
            Some: task => acc.Add(id, FloatOf(task, byId, early, late)),
            None: () => acc));
    }

    private static HashMap<string, (LocalDate Es, LocalDate Ef)> Forward(Seq<string> order, HashMap<string, ScheduleTask> byId) =>
        order.Fold(HashMap<string, (LocalDate Es, LocalDate Ef)>(), (acc, id) => byId.Find(id).Match(
            Some: task => task.Predecessors.IsEmpty
                ? acc.Add(id, (task.PlannedStart, task.PlannedStart.Plus(Period.FromDays((int)task.PlannedDuration.Days))))
                : task.Predecessors.Fold((Es: task.PlannedStart, Set: false), (state, rel) => acc.Find(rel.Predecessor).Match(
                        Some: pred => EarlyStart(rel, pred, task) is var candidate && (!state.Set || candidate > state.Es) ? (candidate, true) : state,
                        None: () => state)) is var folded
                    && folded.Es.Plus(Period.FromDays((int)task.PlannedDuration.Days)) is var ef
                    ? acc.Add(id, (folded.Es, ef))
                    : acc,
            None: () => acc));

    private static LocalDate EarlyStart(TaskRelation rel, (LocalDate Es, LocalDate Ef) pred, ScheduleTask task) {
        var lag = Period.FromDays((int)rel.Lag.Days);
        var span = Period.FromDays((int)task.PlannedDuration.Days);
        return rel.Kind.Switch(
            finishToStart: () => pred.Ef.Plus(lag),
            startToStart:  () => pred.Es.Plus(lag),
            finishToFinish: () => pred.Ef.Plus(lag).Minus(span),
            startToFinish:  () => pred.Es.Plus(lag).Minus(span));
    }

    private static HashMap<string, (LocalDate Ls, LocalDate Lf)> Backward(
        Seq<string> reverse, HashMap<string, ScheduleTask> byId, HashMap<string, (LocalDate Es, LocalDate Ef)> early) {
        var successors = byId.Values.Fold(HashMap<string, Seq<TaskRelation>>(), (acc, task) =>
            task.Predecessors.Fold(acc, (inner, rel) =>
                inner.AddOrUpdate(rel.Predecessor, held => held.Add(rel with { Successor = task.TaskId }), Seq(rel with { Successor = task.TaskId }))));
        var horizon = early.Values.Fold(LocalDate.MinIsoValue, static (max, slot) => slot.Ef > max ? slot.Ef : max);
        return reverse.Fold(HashMap<string, (LocalDate Ls, LocalDate Lf)>(), (acc, id) => byId.Find(id).Match(
            Some: task => successors.Find(id).Match(
                Some: edges => edges.Fold((Lf: horizon, Set: false), (state, rel) => byId.Find(rel.Successor).Match(
                            Some: succ => acc.Find(rel.Successor).Match(
                                Some: lateSucc => LateFinish(rel, lateSucc, task, succ) is var candidate && (!state.Set || candidate < state.Lf) ? (candidate, true) : state,
                                None: () => state),
                            None: () => state)) is var folded
                        && folded.Lf.Minus(Period.FromDays((int)task.PlannedDuration.Days)) is var ls
                        ? acc.Add(id, (ls, folded.Lf))
                        : acc,
                None: () => acc.Add(id, (horizon.Minus(Period.FromDays((int)task.PlannedDuration.Days)), horizon))),
            None: () => acc));
    }

    private static LocalDate LateFinish(TaskRelation rel, (LocalDate Ls, LocalDate Lf) succ, ScheduleTask pred, ScheduleTask successor) {
        var lag = Period.FromDays((int)rel.Lag.Days);
        var span = Period.FromDays((int)pred.PlannedDuration.Days);
        return rel.Kind.Switch(
            finishToStart: () => succ.Ls.Minus(lag),
            startToStart:  () => succ.Ls.Minus(lag).Plus(span),
            finishToFinish: () => succ.Lf.Minus(lag),
            startToFinish:  () => succ.Lf.Minus(lag).Plus(span));
    }

    private static ScheduleFloat FloatOf(
        ScheduleTask task, HashMap<string, ScheduleTask> byId,
        HashMap<string, (LocalDate Es, LocalDate Ef)> early, HashMap<string, (LocalDate Ls, LocalDate Lf)> late) {
        var (es, ef) = early.Find(task.TaskId).IfNone((task.PlannedStart, task.PlannedFinish));
        var (ls, lf) = late.Find(task.TaskId).IfNone((task.PlannedStart, task.PlannedFinish));
        var totalFloat = Period.Between(es, ls, PeriodUnits.Days).Days;
        var freeFloat = byId.Values
            .Filter(succ => succ.Predecessors.Exists(rel => rel.Predecessor == task.TaskId))
            .Fold(Option<int>.None, (held, succ) => early.Find(succ.TaskId).Match(
                Some: slot => Some(Period.Between(ef, slot.Es, PeriodUnits.Days).Days) is var gap && held.Map(prior => gap.IfNone(prior) < prior).IfNone(true) ? gap : held,
                None: () => held))
            .IfNone(totalFloat);
        return new ScheduleFloat(task.TaskId, es, ef, ls, lf, Duration.FromDays(int.Max(0, totalFloat)), Duration.FromDays(int.Max(0, freeFloat)));
    }

    private static Seq<string> TopoOrder(Seq<ScheduleTask> network, HashMap<string, ScheduleTask> byId) {
        var indegree = network.Fold(network.ToHashMap(static t => t.TaskId, static _ => 0),
            (acc, task) => task.Predecessors.Fold(acc, (inner, _) => inner.AddOrUpdate(task.TaskId, d => d + 1, 1)));
        var ready = toSeq(indegree.Filter(static (_, d) => d == 0).Keys);
        return Drain(ready, indegree, byId, Seq<string>());
    }

    private static Seq<string> Drain(Seq<string> ready, HashMap<string, int> indegree, HashMap<string, ScheduleTask> byId, Seq<string> ordered) =>
        ready.HeadOrNone().Match(
            Some: head => byId.Values.Filter(t => t.Predecessors.Exists(rel => rel.Predecessor == head))
                    .Fold((Degree: indegree, Ready: ready.Tail), (state, succ) =>
                        state.Degree.AddOrUpdate(succ.TaskId, d => d - 1, 0) is var dropped
                            ? (dropped, dropped.Find(succ.TaskId).IfNone(1) == 0 ? state.Ready.Add(succ.TaskId) : state.Ready)
                            : state) is var stepped
                ? Drain(stepped.Ready, stepped.Degree, byId, ordered.Add(head))
                : ordered,
            None: () => ordered);
}

[SmartEnum]
public sealed partial class RowKind {
    public static readonly RowKind Task = new();
    public static readonly RowKind Relation = new();
    public static readonly RowKind Wbs = new();
}
```

| [INDEX] | [FORMAT]       | [SURFACE]                          | [TABLES]                                               |
| :-----: | :------------- | :--------------------------------- | :----------------------------------------------------- |
|  [01]   | P6 XER         | Sep tab-delimited multi-table read | `TASK` activities, `TASKPRED` relations, `PROJWBS` WBS |
|  [02]   | MS-Project XML | STJ source-generated read          | `Task` elements, `PredecessorLink` relations           |

| [INDEX] | [ALGEBRA]           | [SURFACE]                                                     | [LAW]                                                                    |
| :-----: | :------------------ | :------------------------------------------------------------ | :----------------------------------------------------------------------- |
|  [01]   | critical path       | `CriticalPath` = total-float-zero set of `CpmPass.Pass`       | the float-zero activity set, never the single longest task               |
|  [02]   | float pass          | `CpmPass` forward/backward DAG walk over `TaskRelation`       | FS/SS/FF/SF dispatched through the `[SmartEnum]` total `Switch` with lag |
|  [03]   | baseline            | `ScheduleBaseline.Of` `XxHash128` over edition set            | multi-baseline variance is a content-key diff, never a table             |
|  [04]   | retained/progressed | `ActualStart`/`ActualFinish` `Option<LocalDate>` vs data date | reads existing columns; no new field                                     |
|  [05]   | resource loading    | `ResourceId`/`ResourceUnits`/`BudgetedCost` columns           | leveling and over-allocation are activity-network folds                  |

## [03]-[TS_PROJECTION]

- Owner: `ScheduleFormatKind`, `ScheduleTaskWire`, `TaskRelationWire`, `RelationKindWire`, `ScheduleFloatWire`, `ScheduleBaselineWire` — the durable external-scheduler wire surface the TS-web Gantt and the CPM float overlay decode; the 4D construction-state wire shapes (task-element link, per-element status, as-of state map) are the `Rasm.Bim/schedule` owner's projection the scrubber decodes from the Bim wire, never re-declared here.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface.
- Boundary: dates cross as ISO-8601 date strings (NodaTime `LocalDate` round-trip); durations cross as the roundtrip-pattern string; the relation kind crosses as the two-letter code and reconstructs as the literal union; the WBS path crosses as the ltree path string so the Gantt renders the hierarchy; the float and baseline shapes cross primitive-mapped so the CPM overlay renders the critical path; the 4D construction-state wire (the task-element link, the per-element status, and the as-of state map) is the Bim schedule owner's projection — the scrubber decodes it from the Bim wire and federates the durable external float overlay to it by the IFC `GlobalId` the `ScheduleTaskWire.globalId` carries (the durable `TaskId` is the P6/MS-Project activity code, never the federation key), so this surface carries only the durable external-scheduler shapes and never a 4D-state record.

```ts contract
type ScheduleFormatKind = "p6-xer" | "ms-project-xml";

type RelationKindWire = "FS" | "SS" | "FF" | "SF";

interface TaskRelationWire {
  predecessor: string;
  successor: string;
  kind: RelationKindWire;
  lag: string;
}

interface ScheduleTaskWire {
  taskId: string;
  globalId: string | null;
  wbsPath: string;
  name: string;
  plannedStart: string;
  plannedFinish: string;
  actualStart: string | null;
  actualFinish: string | null;
  plannedDuration: string;
  percentComplete: number;
  predecessors: TaskRelationWire[];
  resourceId: string;
  resourceUnits: number;
  budgetedCost: number;
}

interface ScheduleFloatWire {
  taskId: string;
  earlyStart: string;
  earlyFinish: string;
  lateStart: string;
  lateFinish: string;
  totalFloat: string;
  freeFloat: string;
  critical: boolean;
}

interface ScheduleBaselineWire {
  name: string;
  edition: Uint8Array;
  at: string;
}
```

## [04]-[RESEARCH]

- [XER_TABLE_GRAMMAR]: the P6 XER `%T`/`%F`/`%R`/`%E` table-marker grammar and the `TASK`/`TASKPRED`/`PROJWBS` column names the Sep header-named projection reads — the activity-id, planned/actual date, duration, and percent-complete columns and the `TASKPRED` predecessor/successor/lag columns, verified against a real XER export before the reader fence pins the column projection; the IFC `GlobalId` federation correlation reads the `TASK` user-defined-field column (the P6 `UDFVALUE`/`UDFTYPE` IFC-GUID mapping a P6 activity to its `Rasm.Bim/schedule` `ConstructionTask`), `Option<string>.None` when the export carries no IFC-GUID UDF, so the durable external float overlay federates to the host-neutral CPM/4D domain by `GlobalId` and never by the P6 activity `TaskId`.
- [MSPROJECT_XML_SHAPE]: the MS-Project XML `Project`/`Tasks`/`Task`/`PredecessorLink` element shape and date encoding the STJ source-generated reader transcribes, verified against a real `.xml` export, and the Compute P6/XER parse companion's canonical-activity-bytes hand-off boundary; the IFC `GlobalId` federation correlation reads the `Task` `ExtendedAttribute`/`Value` IFC-GUID custom field (`Option<string>.None` when absent) so the wire join keys the durable overlay to the `Rasm.Bim/schedule` `ConstructionTask.GlobalId`; the 4D construction-state fold over the IFC-projected `SequenceRel` network is the `Rasm.Bim/schedule` owner's concern joined at the wire by that `GlobalId`.
