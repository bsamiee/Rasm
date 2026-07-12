# [PERSISTENCE_INGEST_SCHEDULE]

Rasm.Persistence ingests and emits project-schedule files through ONE `ScheduleSource` owner over the `MPXJ.Net` interchange codec: ~20 read dialects (P6 `XER`/`PMXML`, MS Project `.mpp`/`MSPDI`/`MPX`, Asta, Phoenix, Planner, SDEF, …) materialize into the neutral `ProjectFile` graph through ONE format-sniffing ingress — `new UniversalProjectReader().ReadAll(string|Stream)` — and the WRITE half serializes back OUT through ONE egress — `new UniversalProjectWriter(FileFormat).Write(ProjectFile|IList<ProjectFile>, …)` — over the seven writable `FileFormat` members the `ScheduleFormat` `[SmartEnum<string>]` freezes, so the durable store round-trips a P6/MS-Project schedule in both directions and never truncates a multi-project XER container: the ingress is ALWAYS `ReadAll` (a single-project file is the one-element container, arity recoverable from the yield, never a `multi` knob), and the egress arity is the graph's own count (`Write(file)` at one, `Write(files, …)` past one). The persisted payload is the durable activity-network DAG a scheduling store actually keeps — `ScheduleActivity` rows, the predecessor/successor/`DependencyKind`/lag `TaskRelation` rows (the CPM edge set), the working-time `WorkCalendarRow` exception windows, and the `ResourceRow`/`AssignmentRow` loading — reconstructed on the write leg through `Relation.Builder(ProjectFile)` and `Duration.GetInstance(double, TimeUnit)` so the store's rows, not a retained foreign object, are the system of record.

The codec NEVER knows the element graph and NEVER computes schedule math: the per-app schedule→element map (the wire-composition owner at the host/app composition root, mirroring `Ingest/tabular` ARCH:61 row-shape law) projects each activity row into a `Rasm.Element` graph node, and the CPM forward/backward pass, resource leveling, and 4D sequencing live in `Rasm.Bim` (the `TaskRelation` rows are exactly the `SequenceRel` DAG its QuikGraph `SourceFirstTopologicalSort` orders — BIM:102's durable-schedule counterpart, widened read→round-trip). The IKVM boundary is ONE seam: every `MPXJ.Net` proxy carries a `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>`, and `ProjectRows.Of`/`Synthesis.Fold` are the only members that touch a proxy type — the handle never threads into a durable row, durations and lags cross unit-tagged as `ScheduleSpan` (the `TimeUnit` enum name, never an assumed day count), and the four `RelationType` members cross once through the `DependencyKind` row column. Both legs fold every codec exception through one `ScheduleFault.Lift` funnel into `Validation<ScheduleFault, …>` at the row boundary — a null `ReadAll` yield (unrecognized bytes) rails the typed `UnknownDialect`, a parse throw on recognized content rails `CodecReject` — with `Code => FaultBand.Schedule + n` off the `Element/graph#FAULT_TABLES` registry (band 8400) and every fact riding the kind-discriminated `ScheduleFact` stream under `store.schedule.*`. `Origin` (the folder's shared path-vs-stream source `[Union]`) arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_RAIL` (the [A.1] frame — no `ClockPolicy` crosses down); `FaultBand` from `Element/graph#FAULT_TABLES`; `ReceiptSinkPort` from AppHost. `MPXJ.Net` is pinned 16.5.0, LGPL-2.1 satisfied by dynamic-link `PackageReference`, pure-managed post-IKVM-translation, osx-arm64-clean.

## [01]-[INDEX]

- [01]-[SCHEDULE_SOURCE]: the one parse/serialize/probe op family over `ScheduleSpec`, the seven-member `ScheduleFormat` egress axis, the always-`ReadAll` container law, the row-boundary fault fold on both legs, and the typed fact stream.
- [02]-[DURABLE_NETWORK]: the persisted activity/relation/calendar/resource rows, the unit-tagged `ScheduleSpan` crossing, the `DependencyKind` seam column, and the `Relation.Builder`/`Duration.GetInstance` synthesis fold that makes the write leg real.

## [02]-[SCHEDULE_SOURCE]

- Owner: `ScheduleFormat` the `[SmartEnum<string>]` egress axis frozen to the seven writable `FileFormat` members (`XER`/`PMXML` the P6 round-trip, `MSPDI`/`MPX` the MS Project pair, `JSON` the neutral dump, `PLANNER`, `SDEF`) — the read side needs NO format axis because `UniversalProjectReader` format-sniffs, so a read-format vocabulary would be a decorative mirror of the codec's own dispatch; `ScheduleSpec` the `[ComplexValueObject]` fixing the `Origin` source plus the `Option<string>` project selector (a multi-project XER selects one project by title, `None` yields every project — arity lives in the spec value, never a flag); `ScheduleOp` the closed `[Union]` op family (`Parse | Serialize | Probe`) so ingress, egress, and header probe are cases of ONE dispatch, never sibling `Read`/`Write`/`Inspect` entrypoints; `ScheduleYield` the closed result family (`Projects | Written | Profile`); `ScheduleFault` the closed row-boundary fault band deriving `FaultBand.Schedule + n`; `ScheduleFact` the typed receipt-stream record; `ScheduleSource` the static surface owning the one `Run` dispatch.
- Cases: `ScheduleOp.Parse(ScheduleSpec)` reads the container through `ReadAll`, filters by the spec's project selector, and projects each `ProjectFile` into a durable `ScheduleProject`; `ScheduleOp.Serialize(ScheduleSpec, ScheduleFormat, Seq<ScheduleProject>)` synthesizes `ProjectFile`s back from the durable rows and writes them at the target format — the spec's `Source` IS the write target (one value, both directions); `ScheduleOp.Probe(ScheduleSpec)` reads the container and yields only the per-project `ScheduleProfile` roster (dialect, application, title, activity/relation counts) without projecting rows; `ScheduleFault` is `CodecReject` (a throw from a recognized parse or a serialize refusal) or `UnknownDialect` (bytes no reader claims — `ReadAll` yields null/empty); `ScheduleFact` kinds are `parse | write | probe`.
- Entry: `public static IO<Validation<ScheduleFault, ScheduleYield>> Run(ScheduleOp op, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink)` — ONE polymorphic entry discriminating the closed op union through the generated total `Switch` (a new modality is one case that breaks this dispatch at compile time), folding both legs through the `Capture` funnel so the receipt path never sees a thrown codec exception.
- Auto: the parse leg opens the `Origin` (path or caller-owned stream) once, calls `ReadAll` — NEVER the single-project `Read`, which silently truncates a multi-project XER to its first project, and NEVER an extension branch, because the reader format-sniffs — then projects each `ProjectFile` through `ProjectRows.Of`, the ONE IKVM seam: activities from the flat `Tasks` container with the WBS parent threaded from `ChildTasks` reachability, the dependency network from each `Task.Predecessors` (`IList<Relation>` — reading one side of the symmetric pair so an edge lands once), calendars from `Calendars` with `CalendarExceptions` windows, resources and loading from `Resources`/`ResourceAssignments`; the serialize leg folds each `ScheduleProject` through `Synthesis.Fold` — `AddTask` per activity keyed by a `GetTaskByUniqueID` re-lookup for parents, `Relation.Builder(file).PredecessorTask(pred).SuccessorTask(succ).Type(kind.Wire).Lag(lag.Wire).Build()` per relation, `AddCalendar`/`AddResource` per calendar/resource row — then writes through ONE `UniversalProjectWriter(to.Wire)` whose arity is the graph count; the probe leg reads `ProjectProperties` only (`FileType` the parsed source dialect, `FileApplication`, `ProjectTitle`) so a dialect census never pays row projection.
- Receipt: every op rides a `ScheduleFact` through the `ReceiptSinkPort` envelope under `store.schedule.*` — a `parse` fact carrying the source dialect, project count, and activity/relation totals; a `write` fact carrying the target format key and project count; a `probe` fact carrying the dialect roster size — one kind-discriminated stream, never parallel receipt records; the envelope stamps the HLC, the fact carrying `frame.Now()` as its own observation instant.
- Packages: MPXJ.Net (`UniversalProjectReader.ReadAll`, `UniversalProjectWriter(FileFormat).Write` single and `IList` arities, `FileFormat`, `ProjectFile.Tasks`/`ChildTasks`/`Calendars`/`Resources`/`ResourceAssignments`/`ProjectProperties`/`AddTask`/`AddResource`/`AddCalendar`/`GetTaskByUniqueID`, `Task.UniqueID`/`Name`/`Predecessors`/`PercentageComplete`/`TotalSlack`/`FreeSlack`/`ConstraintType`/`ConstraintDate`/`ActualStart`/`ActualFinish`, `Relation.Builder`/`PredecessorTask`/`SuccessorTask`/`Type`/`Lag`, `RelationType`, `Duration.DurationValue`/`Units`/`GetInstance`, `TimeUnit`, `ProjectCalendar.CalendarExceptions`, `ResourceAssignment.Units`/`Work`/`Cost`), Rasm.Persistence (`Element/graph#FAULT_TABLES` `FaultBand`, `Element/graph#STORE_RAIL` `ProjectionContext`, `Ingest/tabular#TABULAR_SOURCE` `Origin`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new writable dialect is one `ScheduleFormat` row carrying its `FileFormat` member (the read side grows upstream, zero rows here); a new durable axis is one row record on `ScheduleProject` (as `AssignmentRow` is); a new dependency semantics is one `DependencyKind` row; a new op modality is one `ScheduleOp` case breaking `Run` at compile time; a new boundary-fault class is one `ScheduleFault` case inside the registry decade; zero new surface — a hand-rolled XER/MSPDI parser, an extension-branched ingress, a `Read`-only lane that truncates containers, a parallel `ReadSchedule`/`WriteSchedule` name family beside the op union, CPM/leveling math inside the codec, or a schedule→element map inside this page is the deleted form because MPXJ owns parse/serialize, `Rasm.Bim` owns the schedule math, the op union owns modality, and the app composition root owns the element projection.
- Boundary: `ScheduleSource` is the ONE schedule-file ingress/egress owner — the spreadsheet/delimited lanes (`Ingest/tabular`) cannot parse a binary MPP or a P6 XER and this page never parses rectangular data, the two codecs projecting into the SAME downstream record rail; the ingress contract is `ReadAll` under format-sniff (the per-format `IProjectReader` roster is upstream detail this page never enumerates, and `GetProjectReaderProxy` is the configure-before-read fallback composed only if a dialect ever demands pre-read configuration — a growth note, not a surface); writes target the seven `FileFormat` members ONLY (no MPP writer exists — a Microsoft read-only binary), so an unreachable target is unrepresentable at the type; the IKVM `JavaObject` handle and every proxy type live inside `ProjectRows`/`Synthesis` and are never re-exposed — durable rows are pure records; durations/lags cross unit-tagged (`ScheduleSpan` carries the `TimeUnit` name; a consumer converts through the unit and working-day arithmetic reads the calendar rows, never a 5-day-week assumption); the LGPL-2.1 posture is dynamic-link `PackageReference`, never static fusion or re-publication; `← Rasm.Bim/schedule` (BIM:102) consumes the durable rows for 4D/5D and `→ Rasm.Element` receives row shape only.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
// `Expected` aliases the KERNEL federation base (never LanguageExt.Common.Expected); the MPXJ `Duration`
// aliases apart from NodaTime's. `Origin` and `FaultBand` arrive from the folder/registry owners.
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

// The four CPM dependency kinds — the ONE seam the foreign `RelationType` wire enum crosses (OWNER_CHOOSER
// row [11]: language enum at the seam only); durable rows carry the smart-enum key, never the ordinal.
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

// --- [MODELS] ---------------------------------------------------------------------------

// A unit-tagged span: P6/MS-Project durations and lags are calendar-dependent magnitudes in a declared
// unit — collapsing to an absolute NodaTime Duration at ingest would silently re-base every elapsed/percent
// unit, so the durable form keeps (magnitude, TimeUnit name) and re-mints through `Duration.GetInstance`.
public readonly record struct ScheduleSpan(double Magnitude, string Unit) {
    public static readonly ScheduleSpan Zero = new(0d, nameof(TimeUnit.Days));
    public static ScheduleSpan From(MpxjDuration? span) =>
        span is null ? Zero : new(span.DurationValue, (span.Units ?? TimeUnit.Days).ToString());
    public MpxjDuration Wire => MpxjDuration.GetInstance(Magnitude, Enum.Parse<TimeUnit>(Unit));
}

public readonly record struct ScheduleActivity(
    int Key, Option<int> Parent, string Name, double Percent, bool Critical,
    Option<Instant> ActualStart, Option<Instant> ActualFinish,
    ScheduleSpan TotalSlack, ScheduleSpan FreeSlack, Option<string> ConstraintKind, Option<Instant> ConstraintAt);

// THE load-bearing durable payload: the predecessor/successor/kind/lag activity-network DAG — the
// `SequenceRel` edge set Rasm.Bim's QuikGraph CPM orders; MPXJ supplies edges, never the forward pass.
public readonly record struct TaskRelation(int Predecessor, int Successor, DependencyKind Kind, ScheduleSpan Lag);

public readonly record struct CalendarException(Option<Instant> From, Option<Instant> To, bool Working);
public readonly record struct WorkCalendarRow(string Name, Seq<CalendarException> Exceptions);
public readonly record struct ResourceRow(int Key, string Name);
public readonly record struct AssignmentRow(int Activity, int Resource, double Units, ScheduleSpan Work, double Cost);

public readonly record struct ScheduleAnchor(string Dialect, string Application, string Title, string ScheduleFrom);

public sealed record ScheduleProject(
    ScheduleAnchor Anchor, Seq<ScheduleActivity> Activities, Seq<TaskRelation> Relations,
    Seq<WorkCalendarRow> Calendars, Seq<ResourceRow> Resources, Seq<AssignmentRow> Assignments);

public readonly record struct ScheduleProfile(string Dialect, string Application, string Title, int Activities, int Relations);

[ComplexValueObject]
public sealed partial class ScheduleSpec {
    public Origin Source { get; }
    public Option<string> Project { get; }

    static Validation<ValidationError, ScheduleSpec> Validate(Origin source, Option<string> project) =>
        Validation<ValidationError, ScheduleSpec>.Success(new ScheduleSpec(source, project));

    // The container filter: a multi-project XER selects by title, `None` keeps every project — the arity
    // of the yield is the container's own, never a `multi` knob beside the spec.
    public Seq<ProjectFile> Selected(Seq<ProjectFile> container) =>
        Project.Match(Some: title => container.Filter(p => p.ProjectProperties.ProjectTitle == title), None: () => container);
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
// Band 8400 (`FaultBand.Schedule`): `UnknownDialect` is the format-sniff miss (`ReadAll` claims nothing),
// `CodecReject` every parse/serialize throw on claimed content — the same one-funnel law as TabularFault.
[Union]
public abstract partial record ScheduleFault : Expected, IValidationError<ScheduleFault> {
    private ScheduleFault() : base() { }
    public sealed record CodecReject(string Detail) : ScheduleFault;
    public sealed record UnknownDialect(string Probe) : ScheduleFault;

    public override int Code => FaultBand.Schedule + Switch(
        codecReject:    static _ => 1,
        unknownDialect: static _ => 2);

    public override string Message => Switch(
        codecReject:    static c => $"<schedule-codec-reject:{c.Detail}>",
        unknownDialect: static c => $"<schedule-unknown-dialect:{c.Probe}>");

    public override string Category => Switch(
        codecReject:    static _ => "Codec",
        unknownDialect: static _ => "Dialect");

    public static ScheduleFault Create(string message) => new CodecReject(message);
    public static ScheduleFault Lift(Exception boundary) => new CodecReject($"{boundary.GetType().Name}:{boundary.Message}");
}

public readonly record struct ScheduleFact(string Kind, string Dialect, int Projects, long Activities, long Relations, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class ScheduleSource {
    public static IO<Validation<ScheduleFault, ScheduleYield>> Run(ScheduleOp op, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        op.Switch(
            state: (frame, sink),
            parse:     static (s, p) => Parsed(p.Spec, s.frame, s.sink),
            serialize: static (s, w) => Serialized(w.Target, w.To, w.Graph, s.frame, s.sink),
            probe:     static (s, p) => Probed(p.Spec, s.frame, s.sink));

    static IO<Validation<ScheduleFault, ScheduleYield>> Parsed(ScheduleSpec spec, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Container(spec).Map(projects =>
            (ScheduleYield)new ScheduleYield.Projects(spec.Selected(projects).Map(ProjectRows.Of))))
        from _ in rows.Match(
            Succ: y => y is ScheduleYield.Projects p
                ? sink(new ScheduleFact("parse", Dialect(p.Rows), p.Rows.Count, p.Rows.Sum(static r => (long)r.Activities.Count), p.Rows.Sum(static r => (long)r.Relations.Count), frame.Now()))
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
        from _ in done.Match(Succ: _ => sink(new ScheduleFact("write", to.Key, graph.Count, 0L, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select done;

    static IO<Validation<ScheduleFault, ScheduleYield>> Probed(ScheduleSpec spec, ProjectionContext frame, Func<ScheduleFact, IO<Unit>> sink) =>
        from roster in IO.lift(() => Container(spec).Map(projects =>
            (ScheduleYield)new ScheduleYield.Profile(spec.Selected(projects).Map(static p => new ScheduleProfile(
                p.ProjectProperties.FileType ?? "", p.ProjectProperties.FileApplication ?? "",
                p.ProjectProperties.ProjectTitle ?? "", p.Tasks.Count, p.Tasks.Sum(static t => t.Predecessors.Count))))))
        from _ in roster.Match(Succ: y => sink(new ScheduleFact("probe", "", (y as ScheduleYield.Profile)?.Roster.Count ?? 0, 0L, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select roster;

    // The always-`ReadAll` container ingress: format-sniff auto-dispatch, never `Read` (which truncates a
    // multi-project XER to project one) and never an extension branch; an unclaimed byte stream yields
    // null/empty and rails the typed `UnknownDialect` rather than a NullReference downstream.
    static Validation<ScheduleFault, Seq<ProjectFile>> Container(ScheduleSpec spec) =>
        Capture(() => spec.Source.Read(
            path:   p => toSeq(new UniversalProjectReader().ReadAll(p) ?? []),
            stream: s => toSeq(new UniversalProjectReader().ReadAll(s) ?? [])))
        .Bind(static projects => projects.IsEmpty
            ? Validation<ScheduleFault, Seq<ProjectFile>>.Fail(new ScheduleFault.UnknownDialect("<no-reader-claimed-input>"))
            : Validation<ScheduleFault, Seq<ProjectFile>>.Success(projects));

    static void Write(ScheduleFormat to, Seq<ProjectFile> files, string path) {
        if (files is [var only]) { new UniversalProjectWriter(to.Wire).Write(only, path); }
        else { new UniversalProjectWriter(to.Wire).Write([.. files], path); }
    }

    static void Write(ScheduleFormat to, Seq<ProjectFile> files, Stream sink) {
        if (files is [var only]) { new UniversalProjectWriter(to.Wire).Write(only, sink); }
        else { new UniversalProjectWriter(to.Wire).Write([.. files], sink); }
    }

    static string Dialect(Seq<ScheduleProject> rows) => rows.Head.Match(Some: static r => r.Anchor.Dialect, None: static () => "");

    internal static Validation<ScheduleFault, TValue> Capture<TValue>(Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: Validation<ScheduleFault, TValue>.Success,
            Fail: e => Validation<ScheduleFault, TValue>.Fail(ScheduleFault.Lift(e.ToException())));
}

// --- [BOUNDARIES] -----------------------------------------------------------------------

// THE one IKVM seam: `Of` reads a live `ProjectFile` proxy into pure durable rows, `Fold` reconstructs one —
// no `JavaObject` handle, proxy list, or MPXJ type survives past these two members.
public static class ProjectRows {
    public static ScheduleProject Of(ProjectFile file) {
        FrozenSet<int> children = file.Tasks.SelectMany(static t => t.ChildTasks.Select(static c => (Parent: t.UniqueID ?? 0, Child: c.UniqueID ?? 0)))
            .Select(static pair => pair.Child).ToFrozenSet();
        FrozenDictionary<int, int> parents = file.Tasks
            .SelectMany(static t => t.ChildTasks.Select(c => (Child: c.UniqueID ?? 0, Parent: t.UniqueID ?? 0)))
            .ToFrozenDictionary(static p => p.Child, static p => p.Parent);
        return new ScheduleProject(
            new ScheduleAnchor(
                file.ProjectProperties.FileType ?? "", file.ProjectProperties.FileApplication ?? "",
                file.ProjectProperties.ProjectTitle ?? "", file.ProjectProperties.ScheduleFrom?.ToString() ?? ""),
            toSeq(file.Tasks).Map(t => new ScheduleActivity(
                t.UniqueID ?? 0,
                parents.TryGetValue(t.UniqueID ?? 0, out int parent) ? Some(parent) : None,
                t.Name ?? "", t.PercentageComplete ?? 0d, t.Critical ?? false,
                Stamp(t.ActualStart), Stamp(t.ActualFinish),
                ScheduleSpan.From(t.TotalSlack), ScheduleSpan.From(t.FreeSlack),
                Optional(t.ConstraintType?.ToString()), Stamp(t.ConstraintDate))),
            toSeq(file.Tasks).SelectMany(static t => t.Predecessors.Select(r => new TaskRelation(
                r.PredecessorTask?.UniqueID ?? 0, r.SuccessorTask?.UniqueID ?? 0,
                DependencyKind.Of(r.Type), ScheduleSpan.From(r.Lag)))).ToSeq(),
            toSeq(file.Calendars).Map(static c => new WorkCalendarRow(
                c.Name ?? "", toSeq(c.CalendarExceptions).Map(static e => new CalendarException(Stamp(e.FromDate), Stamp(e.ToDate), Working: e.Working)))),
            toSeq(file.Resources).Map(static r => new ResourceRow(r.UniqueID ?? 0, r.Name ?? "")),
            toSeq(file.ResourceAssignments).Map(static a => new AssignmentRow(
                a.Task?.UniqueID ?? 0, a.Resource?.UniqueID ?? 0, a.Units ?? 0d, ScheduleSpan.From(a.Work), a.Cost ?? 0d)));
    }

    static Option<Instant> Stamp(DateTime? at) =>
        at is { } value ? Some(Instant.FromDateTimeUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc))) : None;
}

// The write-leg synthesis fold — the members that make the egress row REAL rather than chartered prose:
// WBS children minted THROUGH their parent (`Task.AddTask()` — the `ChildTasks` hierarchy reconstructs from
// the flat `Parent` options, so parentage round-trips), `Relation.Builder` per DAG edge with the unit-tagged
// lag re-minted through `Duration.GetInstance`, `AddCalendar` + `AddCalendarException` windows (a `Working`
// override re-opens the standard shift — the durable row carries the window, not per-exception hours), and
// `Task.AddResourceAssignment(Resource)` loading rows — every durable row family the ingest captures re-emits.
public static class Synthesis {
    // A Working exception re-opens this shift: the durable `CalendarException` persists the window and the
    // working flag, so the re-emitted day carries the standard span, never a silently-non-working override.
    static readonly TimeOnlyRange DefaultShift = new(new TimeOnly(8, 0), new TimeOnly(17, 0));

    public static ProjectFile Fold(ScheduleProject project) {
        ProjectFile file = new();
        foreach (ScheduleActivity root in project.Activities.Filter(static a => a.Parent.IsNone)) {      // Exemption: the IKVM builder seam is imperative by contract — the proxy graph mutates in place
            Grow(file.AddTask(), root, project);
        }
        foreach (TaskRelation edge in project.Relations) {
            _ = new Relation.Builder(file)
                .PredecessorTask(file.GetTaskByUniqueID(edge.Predecessor))
                .SuccessorTask(file.GetTaskByUniqueID(edge.Successor))
                .Type(edge.Kind.Wire)
                .Lag(edge.Lag.Wire)
                .Build();
        }
        foreach (WorkCalendarRow calendar in project.Calendars) {
            ProjectCalendar made = file.AddCalendar();
            made.Name = calendar.Name;
            calendar.Exceptions.Iter(e => {
                ProjectCalendarException window = made.AddCalendarException(Day(e.From), Day(e.To));
                if (e.Working) { window.Add(DefaultShift); }                                             // no ranges = non-working (MPXJ's own discriminant)
            });
        }
        foreach (ResourceRow resource in project.Resources) {
            Resource row = file.AddResource();
            (row.UniqueID, row.Name) = (resource.Key, resource.Name);
        }
        foreach (AssignmentRow loading in project.Assignments) {
            ResourceAssignment made = file.GetTaskByUniqueID(loading.Activity).AddResourceAssignment(file.GetResourceByUniqueID(loading.Resource));
            (made.Units, made.Work, made.Cost) = (loading.Units, loading.Work.Wire, loading.Cost);
        }
        return file;
    }

    // Depth-first re-parenting over the flat Parent map: a child task is minted through its parent so the
    // proxy hierarchy matches the durable rows exactly — relations and assignments then resolve by UniqueID.
    static void Grow(Task task, ScheduleActivity activity, ScheduleProject project) {
        (task.UniqueID, task.Name, task.PercentageComplete) = (activity.Key, activity.Name, activity.Percent);
        project.Activities.Filter(a => a.Parent == Some(activity.Key)).Iter(child => Grow(task.AddTask(), child, project));
    }

    static DateOnly Day(Option<Instant> at) =>
        at.Map(static i => DateOnly.FromDateTime(i.ToDateTimeUtc())).IfNone(DateOnly.MinValue);
}
```

| [INDEX] | [POLICY]           | [VALUE]                                      | [BINDING]                                                         |
| :-----: | :----------------- | :------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | one schedule owner | `ScheduleSource.Run` over `ScheduleOp`       | parse/serialize/probe are cases of ONE dispatch                   |
|  [02]   | container ingress  | always `ReadAll`, format-sniffed             | `Read` truncates multi-project XER; extension branching deleted   |
|  [03]   | egress axis        | `ScheduleFormat` seven writable rows         | no MPP writer exists; unreachable target unrepresentable          |
|  [04]   | egress arity       | graph count selects `Write` overload         | one vs many is the value's shape, never a flag                    |
|  [05]   | durable payload    | `TaskRelation` DAG + calendar/resource rows  | the persisted P6/MS-Project record; CPM math stays `Rasm.Bim`     |
|  [06]   | unit fidelity      | `ScheduleSpan` unit-tagged crossing          | re-minted via `Duration.GetInstance`; never assumed days          |
|  [07]   | IKVM seam          | `ProjectRows.Of` / `Synthesis.Fold`          | proxy types and `JavaObject` never escape the two members         |
|  [08]   | row-boundary fault | `Validation<ScheduleFault, …>` both legs     | `UnknownDialect` from a null `ReadAll`; `CodecReject` from throws |
|  [09]   | fault band         | `Code => FaultBand.Schedule + n`             | 8401-8402 off the `graph#FAULT_TABLES` registry                   |
|  [10]   | receipt            | one `ScheduleFact` stream `store.schedule.*` | kind-discriminated; never parallel receipt records                |
|  [11]   | element projection | per-app schedule→element map                 | codec delivers row shape only (ARCH:61 mirrored)                  |

## [03]-[DURABLE_NETWORK]

- Owner: the durable-row family `[02]` declares — `ScheduleProject` the per-project aggregate, `ScheduleActivity` the WBS-threaded activity row, `TaskRelation` the CPM edge row, `WorkCalendarRow`/`CalendarException` the working-time record, `ResourceRow`/`AssignmentRow` the loading record, `ScheduleAnchor` the project header — persisted through the store rail as the app's element projection dictates; this section owns the PROJECTION LAW, not a second type set.
- Cases: an activity's WBS parentage is the `Parent` option threaded from `ChildTasks` reachability (the flat `Tasks` container plus one parent map — never a recursive durable tree, so the row set is depth-free and bulk-lane-shaped); a dependency edge lands ONCE from the predecessor side of MPXJ's symmetric `Predecessors`/`Successors` pair; a calendar exception window is `(From, To, Working)` so holiday and worked-weekend overrides are one row shape; an assignment is `(Activity, Resource, Units, Work, Cost)` — the 5D loading row `Rasm.Bim`'s cost network lifts (each raw `double` becomes `Money`/`UnitsNet` at ITS boundary, never here).
- Entry: NONE — the rows are values the `[02]` ops yield and accept; the store-rail write is the app's (`Element/graph#STORE_RAIL`), the columnar/bulk landing is `Ingest/tabular#BULK_LANE`'s three-row boundary law applied to these typed rows.
- Auto: round-trip fidelity is structural — `ProjectRows.Of` then `Synthesis.Fold` reconstructs the WBS-parented activity hierarchy (children minted through `Task.AddTask()` off the flat `Parent` options), the full relation DAG with kinds and unit-tagged lags, the calendars WITH their exception windows, the resources, and the resource-assignment loading rows, so a P6 XER ingested and re-serialized as XER preserves the network byte-meaningfully (per-exception hour ranges re-open as the standard shift — the one durable-row narrowing; field-level fidelity beyond the durable row set — baselines, activity codes, custom fields — widens as rows on `ScheduleActivity`, never as a retained `ProjectFile`); slack rows (`TotalSlack`/`FreeSlack`) persist as PARSED evidence of the source tool's last CPM pass, never recomputed here.
- Receipt: none of its own — the rows ride `[02]`'s facts.
- Packages: covered by `[02]`.
- Growth: a baseline set is one `Seq<BaselineRow>` field on `ScheduleActivity`; an activity-code assignment is one `HashMap<string,string>` row; a constraint vocabulary hardening is `ConstraintKind` graduating from string evidence to a `[SmartEnum<string>]` when a consumer dispatches on it; zero new surface — a durable `ProjectFile` blob, a per-dialect row family, or a recomputed-slack column is the deleted form because the neutral row set IS the durable record and the source tool's arithmetic is evidence, not authority.
- Boundary: the rows are the Persistence half of the relocated Bim schedule domain — `Rasm.Bim` runs CPM/4D over them (BIM:102, read AND round-trip: an edited durable network serializes back to XER/PMXML through `[02]`'s egress so the P6 authoring tool re-imports it); the app composition root maps activity rows to `Rasm.Element` nodes (row-shape law); no row carries an MPXJ type, a Java handle, or an absolute re-based duration.

| [INDEX] | [POLICY]    | [VALUE]                                | [BINDING]                                                    |
| :-----: | :---------- | :------------------------------------- | :----------------------------------------------------------- |
|  [01]   | WBS shape   | flat rows + `Parent` option            | depth-free, bulk-lane-shaped; never a recursive durable tree |
|  [02]   | edge dedup  | predecessor-side read only             | MPXJ's symmetric pair lands once                             |
|  [03]   | slack rows  | parsed evidence, never recomputed      | the source tool's CPM pass is evidence, not authority        |
|  [04]   | 5D crossing | raw doubles on `AssignmentRow`         | `Money`/`UnitsNet` lift at the Bim boundary, never here      |
|  [05]   | round-trip  | `Of` then `Fold` preserves the network | BIM:102 widened read→round-trip; XER out re-imports in P6    |
