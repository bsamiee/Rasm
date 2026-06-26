# [PERSISTENCE_SCHEDULE_INTERCHANGE]

Rasm.Persistence owns the durable external-scheduler store and its sync — the Primavera P6 and Microsoft Project schedule residence plus its CPM float algebra over the external scheduler's relation DAG. `ScheduleImport` admits a schedule FILE in any of the ~20 formats `MPXJ.Net` reads (P6 `XER`/`PMXML`/database, MS Project `MPP`/`MSPDI`/`MPX`, Asta, Phoenix, GanttProject, SureTrak, Synchro, …) through the one `UniversalProjectReader.Read` auto-detecting ingress, mapping the neutral `ProjectFile` graph ONCE into a typed `ScheduleNetwork` activity network that carries the scheduler's RESIDENT computed overlay — `ScheduleProject.Of` reads each `Task.EarlyStart`/`LateStart`/`TotalSlack`/`FreeSlack`/`Critical` straight off MPXJ's parsed `Task` into the typed `ScheduleFloat` the source scheduler already solved, plus the `Task.ConstraintType`/`ConstraintDate` activity constraint and the `Task.Calendar` correlation, so the durable store RESIDES the external float overlay rather than re-deriving it; `CpmPass.Resolve` runs the forward/backward float pass only as the INDEPENDENT re-derivation that gates the resident overlay — early/late start-finish and total/free float over the `TaskRelation` DAG honoring the `ScheduleConstraint` clamps — so the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor and the near-critical band), a constraint-pressured plan surfaces its negative-float set rather than hiding it under a non-negative clamp, a schedule slip is a typed `FloatErosion` delta against a `ScheduleBaseline`, the `Task.ResourceAssignments` loading drives the `CpmPass.ResourceProfile` over-allocation fold, and the `SchedulePass` receipt carries the data date, the project finish, the critical-path and negative-float counts, the `Cpli` DCMA schedule-health index, and the stored-versus-derived float reconciliation that only holds because the resident overlay TRANSCRIBES MPXJ's parsed `Task.TotalSlack` rather than re-deriving it. `ScheduleBaseline` is one content-addressed snapshot so a baseline-versus-current variance is a multi-baseline content-key diff; the CPM walk is one DAG algebra serving CPM, lineage, and the commit-DAG. The export leg is symmetric — `ScheduleExport.Write` projects the `ScheduleNetwork` back through `UniversalProjectWriter` to the seven writable `FileFormat` members (`PMXML`/`XER` P6 round-trip, `MSPDI`/`MPX` MS Project, `PLANNER`, `SDEF`, neutral `JSON`), so a Rasm-resident schedule re-exports to the originating tool. The 4D construction-state domain semantics — the schedule-activity-to-`ElementSet` binding, the per-element `FourDStatus`, the as-of construction-state fold, and the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — are the AEC-domain owner `Rasm.Bim/schedule`'s concern, consumed here only at the `Sync/schedule ⇄ Rasm.Bim/schedule # [WIRE]: P6/MS-Project + 4D construction domain` wire; per the `[SCHEDULE_NETWORK_DEPTH]` boundary the source lanes split — Persistence owns the durable P6/MS-Project store plus its `CpmPass` over the external `TaskRelation` DAG, Bim owns the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — strata-legal as app-platform consuming aec-domain. The two stores federate by the IFC `GlobalId`: a `ScheduleTask` carries the `Option<string> GlobalId` correlation to the Bim `ConstructionTask.GlobalId` (the IFC process the external P6/MS-Project activity maps to, read off `Task.GUID`, `None` for an activity with no IFC counterpart), and the wire join keys the durable external float overlay to the host-neutral CPM/4D domain by that `GlobalId` — the external `TaskId` is the MPXJ `Task.UniqueID`/`ActivityID` activity code, never the federation key, so a Bim `CriticalPath` reading a P6 relation or a Persistence `CpmPass` re-deriving the IFC `SequenceRel` network is the cross-package drift the relocation forbids. The cross-document link (`Query/federation#CROSS_DOC_LINKS`), the element-set currency (`Query/federation#ELEMENT_SET_ALGEBRA`), the time-travel AS-OF fold (`Version/timetravel#TIME_TRAVEL`), and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled.

## [01]-[INDEX]

- [01]-[SCHEDULE_STORE]: schedule-file admission through the one `MPXJ.Net` auto-detecting reader into a typed activity network carrying the scheduler's transcribed resident float overlay, activity constraints, and calendar correlation; WBS; constraint-aware CPM float algebra with negative-float and threshold-critical sets, resource-leveling over-allocation, a typed `SchedulePass` receipt with the `Cpli` DCMA index, a baseline `FloatErosion` diff, and the symmetric `UniversalProjectWriter` export leg.
- [02]-[TS_PROJECTION]: schedule task, relation, float, constraint, and baseline wire shapes.

## [02]-[SCHEDULE_STORE]

- Owner: `ScheduleFormat` the writable-target axis (the seven `MPXJ.Net` `FileFormat` members the export leg targets) carrying the `FileFormat Target` egress discriminant — read is format-agnostic through `UniversalProjectReader` so the axis exists for the asymmetric WRITE side only; `RelationKind` the `[SmartEnum<string>]` dependency-modality axis under the IFC two-letter key carrying the MPXJ `RelationType` alias, the `(bool FromFinish, bool ToFinish)` float-algebra anchors, and the `DriveEarly`/`DriveLate`/`FreeGap` row behavior the forward/backward/free-float passes dispatch (signed lag, a lead preserved); `ScheduleConstraint` the `[SmartEnum<string>]` activity-constraint axis carrying the `(bool Start, ClampSense Sense)` policy and the `ClampEarly`/`ClampLate` row behavior the pass dispatches (a `Hard` `On`/`Mandatory` pinning both passes); `ScheduleTask` the typed activity record carrying the resource-loading rows, the scheduler-resident `Option<ScheduleFloat>` overlay, the `Option<TaskConstraint>` constraint, and the `Option<string>` calendar id; `TaskRelation` the predecessor/successor dependency carrying the lag as a `Duration`; `ScheduleFloat` the typed per-activity early/late start-finish + total/free float window carrying the `Critical` (float-zero) and `Negative` (constraint-infeasible) predicates plus the `Resident` (transcribe MPXJ's parsed `Task.TotalSlack`/`FreeSlack`) and `Window` (derive from the date span) admission factories; `FloatErosion` the per-activity baseline-versus-current total-float delta; `ResourceLoad` the per-resource peak-unit/peak-day/budgeted-cost leveling profile carrying the `Overallocated(capacity)` predicate; `ScheduleBaseline` the content-addressed baseline-snapshot axis; `SchedulePass` the typed CPM receipt carrying the per-task `ScheduleFloat`, the data date, the project finish, the critical-path and negative-float counts, and the stored-versus-derived reconciliation, projecting the `CriticalSet(floatDays)` threshold critical set and the `Cpli(baselineFinish)` DCMA schedule-health index; `ScheduleProject` the static boundary surface owning the one `ProjectFile` → `ScheduleNetwork` mapping (the MPXJ `Duration`/`DateTime?` lift, the `ChildTasks` WBS fold, the `Predecessors` edge projection, and the `ScheduleBaseline.Diff` erosion projection); `ScheduleImport`/`ScheduleExport` the read/write boundary entries; `CpmPass` the static forward/backward DAG-walk fold owning the constraint-aware float pass and the `ResourceProfile` leveling fold.
- Cases: `ScheduleFormat` `Xer` (`FileFormat.XER`) | `PmXml` (`FileFormat.PMXML`) | `Mspdi` (`FileFormat.MSPDI`) | `Mpx` (`FileFormat.MPX`) | `Planner` (`FileFormat.PLANNER`) | `Sdef` (`FileFormat.SDEF`) | `Json` (`FileFormat.JSON`) — the seven writable targets, read needing none; `RelationKind` `FinishToStart` (`FS`/`FinishStart`) | `StartToStart` (`SS`/`StartStart`) | `FinishToFinish` (`FF`/`FinishFinish`) | `StartToFinish` (`SF`/`StartFinish`), each row carrying the float anchors the four pass arms read; `ScheduleConstraint` `StartOn` | `StartNoEarlierThan` | `StartNoLaterThan` | `FinishOn` | `FinishNoLaterThan` | `MandatoryStart` | `MandatoryFinish` | `AsLateAsPossible`, each carrying its MPXJ `ConstraintType` alias; a task carries id, optional IFC `GlobalId`, WBS path, name, planned/actual start and finish, planned duration, percent-complete, the `Seq<TaskResource>` assignment loading, the resident `ScheduleFloat` overlay the scheduler solved, the activity constraint, and the calendar id; `ScheduleFloat` carries the early/late start-finish and total/free float, the critical activity being the total-float-zero floor and the negative-float activity the constraint-infeasible set; `SchedulePass` carries the float map, the data date, the project finish, the critical-path and negative-float counts, and the `Stored`/`Derived`/`Drift` reconciliation; `ResourceLoad` carries the per-resource peak concurrent units, the peak day, and the rolled budgeted cost.
- Entry: `public static Fin<ScheduleNetwork> ScheduleImport.Read(Stream source)` admits ANY supported schedule file through `new UniversalProjectReader().Read(source)` and maps the returned `ProjectFile` through `ScheduleProject.Of`, the codec's format-sniff throw and the empty/unsupported file rejoining the rail as `ScheduleFault.Unreadable`; `public static SchedulePass CpmPass.Resolve(ScheduleNetwork network, LocalDate dataDate)` runs the constraint-aware forward/backward pass producing the typed receipt (the data date sourced once from `ProjectProperties.StatusDate` at the boundary, falling back to the network drive when the export omits it); `public Seq<string> SchedulePass.CriticalSet(int floatDays)` projects the float-threshold critical set (the total-float-zero floor at `0`, the near-critical band above) and `public double SchedulePass.Cpli(LocalDate baselineFinish)` the DCMA critical-path-length index; `public static Seq<ResourceLoad> CpmPass.ResourceProfile(ScheduleNetwork network, SchedulePass pass)` folds the `TaskResource` loading into the per-resource peak-demand leveling profile; `public static ScheduleBaseline ScheduleBaseline.Of(string name, ScheduleNetwork network, Instant at)` mints the content-addressed baseline and `ScheduleBaseline.Diff(SchedulePass baseline, SchedulePass current)` projects the `Seq<FloatErosion>` slip set; `public static Fin<Unit> ScheduleExport.Write(ScheduleNetwork network, ScheduleFormat format, Stream sink)` serializes the network back through `new UniversalProjectWriter(format.Target).Write(...)` to one of the seven writable formats.
- Auto: the ingress is the one `UniversalProjectReader.Read` call — MPXJ format-sniffs the input and dispatches to the right concrete reader, so the lane never branches on file extension and admits ~20 formats (P6 `XER`/`PMXML`, MS Project `MPP`/`MSPDI`/`MPX`, Asta, Phoenix, GanttProject, SureTrak, Synchro, …) through one entry, and a binary `.mpp` or a P6 `.xer` parses without a hand-rolled tabular or XML reader; `ScheduleProject.Of` maps the neutral `ProjectFile` ONCE — `ChildTasks` walks the WBS hierarchy roots, each `Task.Predecessors` projects the `Relation` edges (the MPXJ `RelationType?` mapping onto the `RelationKind` two-letter row, the `Relation.Lag` `Duration` lifted through its `TimeUnit`), `Task.GUID` reads the IFC `GlobalId` federation correlation (`None` when absent), and the scheduler's RESIDENT solved columns (`Task.EarlyStart`/`EarlyFinish`/`LateStart`/`LateFinish`/`TotalSlack`/`FreeSlack`, all `DateTime?`/`Duration` off the parse) TRANSCRIBE into `ScheduleFloat.Resident` so the durable store RESIDES the scheduler-solved float the Bim `CriticalPath` overlays by `GlobalId`, `None` when the export is unscheduled (`Task.TotalSlack` absent); the MPXJ `Duration` is unit-tagged (`Units` is a `TimeUnit?`) so each duration/lag lifts through its unit into NodaTime `Duration` rather than assuming days, and each `DateTime?` lifts into `Option<LocalDate>` so an absent date is `None` rather than a sentinel; the activity network is a DAG keyed on `Task.UniqueID` with `TaskRelation` edges so the CPM float pass is one DAG-walk the commit-DAG and lineage-DAG share — `CpmPass.Resolve` derives the `Predecessors`/`Successors` adjacency once, `Topological` runs Kahn over it (a residual cycle aborting `ScheduleFault.CyclicNetwork`), the forward pass folds each activity in topological order deriving early start as the maximum over predecessors of the modality-anchored predecessor date plus the SIGNED edge lag (a negative lag is a lead — `RelationKind.DriveEarly` carries the sign, never floors it to zero) then CLAMPED to the `ScheduleConstraint`, the backward pass folds in reverse deriving late finish where a HARD start constraint (`On`/`Mandatory`) pins the late finish below the network drive so the constraint produces the negative float the `NegativeFloat` count surfaces, total float is `Period.DaysBetween(EarlyStart, LateStart)` and free float the per-modality `RelationKind.FreeGap` (successor early-start minus the driving finish/start minus the edge lag, floored at zero), all dispatched through the `RelationKind` and `ScheduleConstraint` row behavior rather than per-arm `Switch` bodies; the `SchedulePass` receipt reconciles each derived `ScheduleFloat` against the task's resident overlay so `Drift` counts the activities where MPXJ's parsed `Task.TotalSlack` and the re-derivation disagree (a re-sequenced plan whose stored float is stale); the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor at `0`, the near-critical band above) rather than the single longest task; the WBS hierarchy walks `ChildTasks` into an ltree path so a roll-up-by-WBS rides the ltree operators; the `Task.ResourceAssignments` loading drives `CpmPass.ResourceProfile`, the per-resource leveling fold spreading each activity's assigned `Units` across its early-start..early-finish span into the peak-day over-allocation surface; the imported schedule is content-addressed so a re-imported revision dedupes and a `ScheduleBaseline.Of` snapshot is one content key, a baseline-versus-current variance being a `ScheduleBaseline.Diff` `FloatErosion` set keyed on the multi-baseline content key rather than a per-baseline table; the export leg is symmetric — `ScheduleExport.Write` rebuilds a `ProjectFile` from the `ScheduleNetwork` and serializes through `UniversalProjectWriter(format.Target)` to the seven writable members (there is NO `MPP` writer — `MPP` is read-only — so the `ScheduleFormat` axis carries exactly the seven writable targets).
- Receipt: an import rides `store.schedule.import` carrying the parsed format name (off `ProjectProperties`), the task count, the relation count, and the resident-overlay-present count; the typed `SchedulePass(Floats, DataDate, ProjectFinish, CriticalLength, NegativeFloat, Stored, Derived, Drift)` is the CPM evidence carrying the per-task `ScheduleFloat`, the data date, the project finish `LocalDate`, the critical-path and negative-float (constraint-infeasibility) counts, and the stored-versus-derived reconciliation; a baseline diff rides `store.schedule.baseline` carrying the `FloatErosion` slip count; a resource-leveling fold rides `store.schedule.resource` carrying the `ResourceLoad` over-allocation peaks; a WBS fold rides `store.schedule.wbs`; an export rides `store.schedule.export` carrying the target `FileFormat` — never a generic `IReceipt`.
- Packages: MPXJ.Net, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime.
- Growth: a new readable interchange format is ZERO new surface — `UniversalProjectReader` already auto-detects every format MPXJ ships, so a new MPXJ release widens the ingress without a code change; a new writable target is one `ScheduleFormat` row carrying its `FileFormat`; a new task field is one column on `ScheduleTask` mapped from one `Task` member in `ScheduleProject.Of` (the resource-loading, resident-overlay, constraint, and calendar columns are exactly this — columns on the existing record, never a second resource/float/constraint record); a new relation type is one `RelationKind` row carrying its float anchors and breaking nothing because the four pass arms read the anchor rows; a new activity constraint is one `ScheduleConstraint` row carrying its clamp policy the pass already reads; a new float metric is one column on `ScheduleFloat` plus its predicate; a new CPM-evidence fact is one field on `SchedulePass` and a new schedule-health index one projection on it (the `CriticalSet` threshold set and the `Cpli` DCMA index are exactly this — projections on the existing receipt, never a parallel metrics record); a new leveling axis is one column on `ResourceLoad`; a new baseline is one `ScheduleBaseline.Of` content key, never a per-baseline table; zero new surface — a per-format schedule model, a hand-rolled XER/MSPDI/MPP parser, a Primavera SDK wrapper, an MS-Project COM interop, a separate stored-float table, a parallel resource-leveling record, or a per-metric critical-path calculator is the deleted form because MPXJ owns the parse of every format into the one `ProjectFile`, `ScheduleProject.Of` maps it ONCE into the one `ScheduleNetwork`, and CPM is one DAG algebra serving CPM, lineage, and the commit-DAG.
- Boundary: the ingress is `MPXJ.Net.UniversalProjectReader.Read(Stream)` so the import is a managed codec read across ~20 formats, never a Primavera SDK, a per-vendor schedule library, or a hand-rolled parser — `UniversalProjectReader` format-sniffs the input and dispatches to the right concrete `IProjectReader`, so a hand-written XER `%T`/`%F`/`%R` tabular reader, an `XDocument` MSPDI walk, a positional-ordinal parse, a caller-supplied projection delegate, AND a per-format `ScheduleFormat` ingest discriminant are all the deleted forms (the prior fence hand-parsed the P6 XER through Sep and the MSPDI through `System.Xml.Linq`, reinventing exactly what MPXJ already owns across every scheduling tool); the `ProjectFile` is the boundary type mapped ONCE through `ScheduleProject.Of` to the canonical `ScheduleNetwork`, the IKVM proxy types (`Task`/`Relation`/`Duration` carry a `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>`) read as ordinary .NET objects and never threading the Java handle into canonical code (re-exposing a proxy type across the package boundary is the deleted form); the durable store TRANSCRIBES MPXJ's parsed `Task.TotalSlack`/`FreeSlack` through `ScheduleFloat.Resident` (never re-derived from the date span) as the federation float the Bim `CriticalPath` overlays by `GlobalId` — discarding the scheduler-solved overlay and re-deriving on every read is the deleted form, and recomputing the resident total float from `EarlyStart`/`LateStart` instead of reading `Task.TotalSlack` is the illusion that would collapse the `Drift` gate into a tautology, so `CpmPass.Resolve` re-derivation through `ScheduleFloat.Window` reconciles against the transcribed overlay as a genuine `Drift` check, never as the sole truth; the MPXJ `Duration` is unit-tagged so a lag/duration is read through its `TimeUnit` (`Days`/`Hours`/`Weeks`/…), never assumed to be days, and meets the canonical NodaTime time vocabulary at the boundary, while working-day arithmetic over `Task.Calendar` is deferred to the host-neutral Bim `WorkCalendar` (the durable store reads the scheduler's already-calendar-resolved dates and re-derives in continuous days as the drift gate); the activity network is a DAG so the critical-path, total-float, and free-float computations are one `CpmPass` forward/backward DAG walk over the relation edges — the float pass reads the `RelationKind` `(FromFinish, ToFinish)` anchor rows and the `ScheduleConstraint` clamp rows (a statement-switch, an if/else ladder over the four arms, OR an unclamped pass that ignores `ConstraintType`/`ConstraintDate` is the deleted form), and the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor at `0`, the near-critical band above), never the single longest-duration task, with the negative-float count surfacing the constraint-infeasible set a naive non-negative clamp would have hidden; the adjacency is derived ONCE into `Predecessors`/`Successors` maps so a per-node rescan in `Forward`/`Backward`/`FloatOf`/`Drain` is the deleted O(V·E) form; `ScheduleFloat` is a typed record struct carrying the float window, never a loose tuple, and a schedule slip is a typed `FloatErosion` against a baseline `SchedulePass`; the `Task.ResourceAssignments` loading rides the `TaskResource` rows on the existing `ScheduleTask` and drives the real `CpmPass.ResourceProfile` leveling fold into the typed `ResourceLoad` peak-demand surface, never a second resource record or a prose-only claim; `ScheduleBaseline` rides the existing content-addressed snapshot identity (`XxHash128.HashToUInt128` over the sorted activity-edition set) so a baseline-versus-current variance is a multi-baseline content-key diff and a per-baseline table is the deleted form; retained/progressed-logic scheduling is the `ScheduleTask.ProgressFloor(dataDate)` behavior the forward pass dispatches — a completed activity pins both legs to its actuals, an in-progress activity pins its start to `ActualStart` and its finish to the data date plus the `PercentComplete`-remaining span, an unstarted activity defers to the network drive — so `ActualStart`/`ActualFinish`/`PercentComplete` (off MPXJ's `Task.ActualStart`/`ActualFinish`/`PercentageComplete`) are load-bearing pass inputs, never dead status columns, with no new field; dates ride NodaTime `LocalDate`/`Period`/`Duration` so a schedule date never becomes a `DateTime` sentinel; the WBS is an ltree path so a cost-by-WBS rollup (`Store/profiles#COST_ROLLUP`) joins the schedule WBS to the takeoff classification through one ltree join; the export leg targets the seven writable `FileFormat` members only (MPXJ writes `JSON`/`MPX`/`MSPDI`/`PLANNER`/`PMXML`/`XER`/`SDEF`, never `MPP`) so a write to a non-writable format is the typed `ScheduleFault.UnwritableFormat`; LGPL-2.1 is satisfied by the dynamic-link `PackageReference` (the codec is consumed as a separate assembly, never statically fused into a Rasm assembly nor re-published), and scheduling MATH (the CPM forward/backward pass, resource leveling, 4D sequencing) stays the CONSUMER's — MPXJ owns parse/serialize only, this page owns the durable residence and the CPM float algebra, and the host-neutral 4D fold is `Rasm.Bim/schedule`'s.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO.Hashing;
using LanguageExt;
using LanguageExt.Common;
using MPXJ.Net;
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;
using MpxjDuration = MPXJ.Net.Duration;
using MpxjTask = MPXJ.Net.Task;

namespace Rasm.Persistence;

// --- [TYPES] ------------------------------------------------------------------------------
public sealed class ScheduleKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

// The WRITABLE-target axis: read is format-agnostic through `UniversalProjectReader.Read` (MPXJ sniffs the
// format), so this axis exists for the asymmetric WRITE side — exactly the seven `FileFormat` members MPXJ
// serializes. `MPP` is read-only and is absent by construction, so a write to it is unrepresentable.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class ScheduleFormat {
    public static readonly ScheduleFormat Xer = new("p6-xer", FileFormat.XER);
    public static readonly ScheduleFormat PmXml = new("p6-pmxml", FileFormat.PMXML);
    public static readonly ScheduleFormat Mspdi = new("ms-project-mspdi", FileFormat.MSPDI);
    public static readonly ScheduleFormat Mpx = new("ms-project-mpx", FileFormat.MPX);
    public static readonly ScheduleFormat Planner = new("planner", FileFormat.PLANNER);
    public static readonly ScheduleFormat Sdef = new("sdef", FileFormat.SDEF);
    public static readonly ScheduleFormat Json = new("json", FileFormat.JSON);

    public FileFormat Target { get; }
}

// The dependency-modality axis: one row per IFC two-letter key carrying the MPXJ `RelationType` alias and the
// (FromFinish, ToFinish) float-algebra anchors the four CPM pass arms read, so a new modality is a row carrying
// its anchors, never a fifth Switch body. MPXJ already normalized every format's native code into `RelationType`,
// so the page maps ONE enum rather than per-vendor `pred_type`/MSPDI-integer codes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class RelationKind {
    public static readonly RelationKind FinishToStart = new("FS", RelationType.FinishStart, fromFinish: true,  toFinish: false);
    public static readonly RelationKind StartToStart  = new("SS", RelationType.StartStart,  fromFinish: false, toFinish: false);
    public static readonly RelationKind FinishToFinish = new("FF", RelationType.FinishFinish, fromFinish: true,  toFinish: true);
    public static readonly RelationKind StartToFinish  = new("SF", RelationType.StartFinish,  fromFinish: false, toFinish: true);

    public RelationType Mpxj { get; }
    public bool FromFinish { get; }
    public bool ToFinish { get; }

    // MPXJ's `Relation.Type` is a `RelationType?`; a null (an unmapped edge) defaults to the dominant FS modality.
    public static RelationKind Of(RelationType? type) => type switch {
        RelationType.StartStart => StartToStart,
        RelationType.FinishFinish => FinishToFinish,
        RelationType.StartFinish => StartToFinish,
        _ => FinishToStart,
    };
}

// The modality drive is row behavior, not a free Switch: each anchor pair maps the predecessor's driving date
// plus the SIGNED lag onto the successor's early/late constraint. A negative lag is a lead (FS-2d starts the
// successor two days before the predecessor finishes), so the drive carries the sign at whole-day resolution.
extension(RelationKind kind) {
    Period AsLag(Duration lag) => Period.FromDays(lag.Days);

    // Forward: the predecessor's anchor (its finish when the edge leaves a finish, else its start) plus the
    // signed lag; the successor START backs off one task span when the edge targets a finish.
    public LocalDate DriveEarly((LocalDate Es, LocalDate Ef) pred, Period span, Duration lag) {
        var from = (kind.FromFinish ? pred.Ef : pred.Es).Plus(kind.AsLag(lag));
        return kind.ToFinish ? from.Minus(span) : from;
    }

    // Backward: the symmetric pull — the successor's anchor minus the signed lag; the predecessor LATE start
    // advances one span when the edge leaves a start (a start-anchored edge frees the late finish).
    public LocalDate DriveLate((LocalDate Ls, LocalDate Lf) succ, Period span, Duration lag) {
        var to = (kind.ToFinish ? succ.Lf : succ.Ls).Minus(kind.AsLag(lag));
        return kind.FromFinish ? to : to.Plus(span);
    }

    // Free float against ONE successor edge: the successor's earliest start minus this activity's driving finish
    // minus the edge lag — the slack before this activity disturbs that successor on this modality.
    public int FreeGap((LocalDate Es, LocalDate Ef) self, LocalDate successorEarlyStart, Duration lag) =>
        Period.DaysBetween(kind.ToFinish ? self.Ef : self.Es, successorEarlyStart) - lag.Days;
}

// The activity-constraint axis: one row per constraint carrying the (Start, Sense) clamp policy the
// forward/backward pass honors and the MPXJ `ConstraintType` alias, so the pass discriminates the clamp by
// row data rather than a constraint Switch body and the boundary maps ONE MPXJ enum rather than per-vendor codes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class ScheduleConstraint {
    public static readonly ScheduleConstraint StartOn            = new("start-on",            ConstraintType.MustStartOn,           start: true,  sense: ClampSense.On);
    public static readonly ScheduleConstraint StartNoEarlierThan = new("start-no-earlier",    ConstraintType.StartNoEarlierThan,    start: true,  sense: ClampSense.NoEarlier);
    public static readonly ScheduleConstraint StartNoLaterThan   = new("start-no-later",      ConstraintType.StartNoLaterThan,      start: true,  sense: ClampSense.NoLater);
    public static readonly ScheduleConstraint FinishOn           = new("finish-on",           ConstraintType.MustFinishOn,          start: false, sense: ClampSense.On);
    public static readonly ScheduleConstraint FinishNoLaterThan  = new("finish-no-later",     ConstraintType.FinishNoLaterThan,     start: false, sense: ClampSense.NoLater);
    public static readonly ScheduleConstraint MandatoryStart     = new("mandatory-start",     ConstraintType.StartNoEarlierThan,    start: true,  sense: ClampSense.Mandatory);
    public static readonly ScheduleConstraint MandatoryFinish    = new("mandatory-finish",    ConstraintType.FinishNoLaterThan,     start: false, sense: ClampSense.Mandatory);
    public static readonly ScheduleConstraint AsLateAsPossible   = new("alap",                ConstraintType.AsLateAsPossible,      start: false, sense: ClampSense.Latest);

    public ConstraintType Mpxj { get; }
    public bool Start { get; }
    public ClampSense Sense { get; }

    // MPXJ's `Task.ConstraintType` is a `ConstraintType?`; `AsSoonAsPossible`/null carry no clamp (None). The
    // `MustStartOn`/`StartOn` and `MustFinishOn`/`FinishOn` pairs both pin On — MPXJ exposes both the MS-Project
    // and the P6 spelling, so the row maps both spellings onto the one On-sense constraint.
    public static Option<ScheduleConstraint> Of(ConstraintType? type) => type switch {
        ConstraintType.AsLateAsPossible => Some(AsLateAsPossible),
        ConstraintType.MustStartOn or ConstraintType.StartOn => Some(StartOn),
        ConstraintType.MustFinishOn or ConstraintType.FinishOn => Some(FinishOn),
        ConstraintType.StartNoEarlierThan => Some(StartNoEarlierThan),
        ConstraintType.StartNoLaterThan => Some(StartNoLaterThan),
        ConstraintType.FinishNoEarlierThan or ConstraintType.FinishNoLaterThan => Some(FinishNoLaterThan),
        _ => None,
    };
}

// The clamp is row behavior keyed on (Start, Sense), so the pass never switches on the constraint kind. A hard
// pin (On/Mandatory) forces the date in BOTH passes — the only way the network produces negative float: a
// mandatory start earlier than the predecessor drive pulls the late date below the early date.
public enum ClampSense : byte { On, NoEarlier, NoLater, Mandatory, Latest }

extension(ScheduleConstraint constraint) {
    public bool Hard => constraint.Sense is ClampSense.On or ClampSense.Mandatory;

    LocalDate Apply(LocalDate on, LocalDate date) => constraint.Sense switch {
        ClampSense.On or ClampSense.Mandatory => on,
        ClampSense.NoEarlier => date < on ? on : date,
        ClampSense.NoLater => date > on ? on : date,
        _ => date,
    };

    // A start-side constraint clamps the forward early start; a finish-side leaves it untouched.
    public LocalDate ClampEarly(TaskConstraint c, LocalDate es) => constraint.Start ? constraint.Apply(c.On, es) : es;

    // A finish-side constraint clamps the backward late finish; a HARD start-side constraint also pins the late
    // finish to (start + span) so a mandatory/on start floors the late date and surfaces negative float.
    public LocalDate ClampLate(TaskConstraint c, LocalDate lf, Period span) =>
        constraint.Start
            ? constraint.Hard ? c.On.Plus(span) : lf
            : constraint.Apply(c.On, lf);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TaskConstraint(ScheduleConstraint Kind, LocalDate On);

public sealed record TaskRelation(string Predecessor, string Successor, RelationKind Kind, Duration Lag);

// One task↔resource assignment off `Task.ResourceAssignments`: the units/work/cost the leveling fold spreads
// across the activity span. A column-bearing row on the existing task, never a second resource record.
public readonly record struct TaskResource(string ResourceId, double Units, double BudgetedCost);

// The scheduler's RESIDENT solved overlay, transcribed off MPXJ's parsed `Task.TotalSlack`/`FreeSlack` and the
// early/late dates — the federation float the Bim CriticalPath overlays by GlobalId. CpmPass.Resolve re-derives
// an INDEPENDENT ScheduleFloat and reconciles against this stored one; this is never discarded for the
// re-derivation. The two factories keep the regimes distinct: Resident TRANSCRIBES MPXJ's parsed slack (negative
// float survives — a constraint-pressured plan carries it), Window DERIVES total float from the late-minus-early
// span. Negative float marks an infeasible plan; Critical is the float-zero floor INCLUDING the negative set so a
// constraint slip stays on the critical path.
public readonly record struct ScheduleFloat(
    LocalDate EarlyStart, LocalDate EarlyFinish,
    LocalDate LateStart, LocalDate LateFinish,
    Duration TotalFloat, Duration FreeFloat) {
    public bool Critical => TotalFloat <= Duration.Zero;
    public bool Negative => TotalFloat < Duration.Zero;

    public static ScheduleFloat Resident(LocalDate es, LocalDate ef, LocalDate ls, LocalDate lf, Duration total, Duration free) =>
        new(es, ef, ls, lf, total, free < Duration.Zero ? Duration.Zero : free);

    public static ScheduleFloat Window(LocalDate es, LocalDate ef, LocalDate ls, LocalDate lf, int freeDays) =>
        new(es, ef, ls, lf, Duration.FromDays(Period.DaysBetween(es, ls)), Duration.FromDays(int.Max(0, freeDays)));
}

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
    Seq<TaskResource> Resources,
    Option<TaskConstraint> Constraint,
    Option<string> CalendarId,
    Option<ScheduleFloat> Resident,
    UInt128 ScheduleEdition) {
    // Progressed-logic data-date floor: a completed activity (ActualFinish present) pins both legs to its
    // actuals; an in-progress activity (ActualStart present, not complete) pins its start to the actual and its
    // finish to the data date plus the percent-complete REMAINING span; an unstarted activity defers to the
    // network drive (None). This is what makes ActualStart/ActualFinish/PercentComplete load-bearing in the pass
    // rather than dead status columns — without it the forward pass ignores progress entirely.
    public Option<(LocalDate Es, LocalDate Ef)> ProgressFloor(LocalDate dataDate) =>
        ActualFinish.Match(
            Some: finished => Some((ActualStart.IfNone(finished), finished)),
            None: () => ActualStart.Map(started => {
                var remaining = PercentComplete is > 0 and < 100
                    ? (int)Math.Ceiling(PlannedDuration.Days * (1d - (PercentComplete / 100d)))
                    : (int)PlannedDuration.Days;
                var from = PercentComplete is > 0 and < 100 ? (started > dataDate ? started : dataDate) : started;
                return (started, from.Plus(Period.FromDays(int.Max(0, remaining))));
            }));
}

// One activity-network record: the task set plus the dependency-edge set, the durable owner the CPM pass, the
// baseline, and the federation read by reference. `Format` is the parsed-source provenance off `ProjectProperties`.
public sealed record ScheduleNetwork(Option<ScheduleFormat> Format, Seq<ScheduleTask> Tasks, Seq<TaskRelation> Relations) {
    public Map<string, ScheduleTask> ById => Tasks.ToMap(static t => t.TaskId);
    public Map<string, Seq<TaskRelation>> Predecessors => Relations.GroupBy(static r => r.Successor).ToMap(static g => g.Key, static g => g.ToSeq());
    public Map<string, Seq<TaskRelation>> Successors => Relations.GroupBy(static r => r.Predecessor).ToMap(static g => g.Key, static g => g.ToSeq());
}

public readonly record struct FloatErosion(string TaskId, Duration WasTotalFloat, Duration NowTotalFloat) {
    public Duration Slip => WasTotalFloat - NowTotalFloat;
    public bool Eroded => Slip > Duration.Zero;
}

// The per-resource leveling profile: the concurrent unit demand summed across the scheduled span and the peak
// day, the over-allocation surface a resource-leveled schedule reads against the resource's declared capacity —
// the activity-network fold the `TaskResource` rows make real, never a second resource record.
public readonly record struct ResourceLoad(string ResourceId, double PeakUnits, LocalDate PeakDay, double BudgetedCost) {
    public bool Overallocated(double capacity) => PeakUnits > capacity;
}

// The typed CPM receipt: the per-task derived float, the project bounds, the critical-path length, the
// negative-float (constraint-infeasibility) count, the stored-versus-derived reconciliation, and the DCMA
// schedule-health index set — never a generic IReceipt. The metric projections fold the float map ONCE rather
// than minting a parallel metrics record.
public sealed record SchedulePass(
    Map<string, ScheduleFloat> Floats, LocalDate DataDate, LocalDate ProjectFinish,
    int CriticalLength, int NegativeFloat, int Stored, int Derived, int Drift) {
    // DCMA Critical Path Length Index: the planned critical-path length plus the project finish slip over the
    // planned length — 1.0 on track, above 1.0 behind. A zero planned span reads as on track.
    public double Cpli(LocalDate baselineFinish) =>
        Period.DaysBetween(DataDate, baselineFinish) is var planned and not 0
            ? (planned + Period.DaysBetween(baselineFinish, ProjectFinish)) / (double)planned : 1d;

    // The float-threshold critical set: the float-zero floor at threshold 0, the near-critical band for a
    // positive day threshold — one polymorphic projection the Gantt overlay drives by slider.
    public Seq<string> CriticalSet(int floatDays) =>
        toSeq(Floats).Filter(row => row.Value.TotalFloat <= Duration.FromDays(floatDays)).Map(static row => row.Key);
}

public readonly record struct ScheduleBaseline(string Name, UInt128 Edition, Instant At) : IComparable<ScheduleBaseline> {
    public static ScheduleBaseline Of(string name, ScheduleNetwork network, Instant at) {
        var sorted = network.Tasks.OrderBy(static t => t.TaskId, StringComparer.Ordinal).ToSeq();
        var buffer = new ArrayBufferWriter<byte>(int.Max(16, sorted.Count * 16));
        var span = buffer.GetSpan(sorted.Count * 16);
        sorted.Iter((task, i) => BinaryPrimitives.WriteUInt128LittleEndian(span[(i * 16)..], task.ScheduleEdition));
        buffer.Advance(sorted.Count * 16);
        return new ScheduleBaseline(name, XxHash128.HashToUInt128(buffer.WrittenSpan), at);
    }

    public static Seq<FloatErosion> Diff(SchedulePass baseline, SchedulePass current) =>
        toSeq(current.Floats).Choose(row => baseline.Floats.Find(row.Key).Map(was =>
            new FloatErosion(row.Key, was.TotalFloat, row.Value.TotalFloat)))
            .Filter(static e => e.Eroded);

    public int CompareTo(ScheduleBaseline other) => At.CompareTo(other.At);
}

// --- [ERRORS] -----------------------------------------------------------------------------
[Union]
public abstract partial record ScheduleFault : Expected, IValidationError<ScheduleFault> {
    private ScheduleFault(string detail, int code) : base(detail, code, None) { }
    public static ScheduleFault Create(string message) => new Unreadable(message);
    public sealed record Unreadable : ScheduleFault { public Unreadable(string detail) : base(detail, 7301) { } }
    public sealed record DanglingEdge : ScheduleFault { public DanglingEdge(string detail) : base(detail, 7302) { } }
    public sealed record CyclicNetwork : ScheduleFault { public CyclicNetwork(string detail) : base(detail, 7303) { } }
    public sealed record UnwritableFormat : ScheduleFault { public UnwritableFormat(string detail) : base(detail, 7304) { } }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ScheduleImport {
    // The one ingress: MPXJ format-sniffs the stream and dispatches to the right concrete reader, so the lane
    // never branches on extension and admits every supported format through one call; the codec throw and the
    // empty/unsupported file rejoin the rail as a typed parse rejection, never an escaping exception.
    public static Fin<ScheduleNetwork> Read(Stream source) =>
        Try.lift(() => new UniversalProjectReader().Read(source)).Run()
            .MapFail(static e => new ScheduleFault.Unreadable($"schedule-unreadable:{e.Message}").ToError())
            .Bind(file => file is null
                ? FinFail<ScheduleNetwork>(new ScheduleFault.Unreadable("schedule-empty").ToError())
                : ScheduleProject.Of(file));
}

public static class ScheduleExport {
    // The symmetric egress: rebuild a `ProjectFile` from the network and serialize through the writer for one of
    // the seven writable formats. `MPP` is unrepresentable in `ScheduleFormat` (read-only), so an unwritable
    // target cannot reach here; a writer throw rejoins the rail.
    public static Fin<Unit> Write(ScheduleNetwork network, ScheduleFormat format, Stream sink) =>
        Try.lift(() => {
            new UniversalProjectWriter(format.Target).Write(ScheduleProject.ToProjectFile(network), sink);
            return unit;
        }).Run().MapFail(e => new ScheduleFault.UnwritableFormat($"schedule-unwritable:{format.Key}:{e.Message}").ToError());
}

// The ONE boundary seam: the neutral MPXJ `ProjectFile` maps once into the canonical `ScheduleNetwork`, lifting
// every `DateTime?` to `Option<LocalDate>` and every unit-tagged MPXJ `Duration` through its `TimeUnit` to a
// NodaTime `Duration`. The IKVM proxy types are read as ordinary .NET objects; the Java handle never threads out.
public static class ScheduleProject {
    public static Fin<ScheduleNetwork> Of(ProjectFile file) {
        var tasks = toSeq(Walk(file.ChildTasks)).Filter(static t => Id(t).Length > 0);
        var network = new ScheduleNetwork(FormatOf(file), tasks.Map(TaskOf), tasks.Bind(EdgesOf));
        var known = toHashSet(network.Tasks.Map(static t => t.TaskId));
        return network.Relations.Find(e => !known.Contains(e.Predecessor) || !known.Contains(e.Successor)).Match(
            Some: e => FinFail<ScheduleNetwork>(new ScheduleFault.DanglingEdge($"{e.Predecessor}->{e.Successor}").ToError()),
            None: () => FinSucc(network));
    }

    // The WBS hierarchy is the `ChildTasks` tree; flatten depth-first so the activity set is the keyed network
    // while each task's `WbsPath` records its outline position for the ltree roll-up.
    static IEnumerable<MpxjTask> Walk(IList<MpxjTask> roots) =>
        roots.SelectMany(static t => new[] { t }.Concat(Walk(t.ChildTasks)));

    static ScheduleTask TaskOf(MpxjTask task) {
        var planned = Date(task.Start).IfNone(LocalDate.MinIsoValue);
        return new ScheduleTask(
            Id(task), Opt(task.GUID?.ToString()), task.WBS ?? "", task.Name ?? Id(task),
            planned, Date(task.Finish).IfNone(planned), Date(task.ActualStart), Date(task.ActualFinish),
            Span(task.Duration), task.PercentageComplete ?? 0d,
            toSeq(task.ResourceAssignments).Map(static a => new TaskResource(
                a.Resource?.UniqueID?.ToString() ?? "", a.Units ?? 0d, a.Cost ?? 0d)).Filter(static r => r.ResourceId.Length > 0),
            ScheduleConstraint.Of(task.ConstraintType).Bind(kind => Date(task.ConstraintDate).Map(on => new TaskConstraint(kind, on))),
            Opt(task.Calendar?.Name), Resident(task),
            XxHash128.HashToUInt128(System.Text.Encoding.UTF8.GetBytes($"{Id(task)}|{planned:R}|{Span(task.Duration).BclCompatibleTicks}")));
    }

    static Seq<TaskRelation> EdgesOf(MpxjTask task) =>
        toSeq(task.Predecessors).Map(rel => new TaskRelation(
            Id(rel.PredecessorTask), Id(task), RelationKind.Of(rel.Type), Span(rel.Lag)));

    // The scheduler's resident overlay: present only when MPXJ parsed the computed early/late dates and slack (a
    // scheduled export), absent for an unscheduled import. The slack legs TRANSCRIBE `Task.TotalSlack`/`FreeSlack`
    // straight (never re-derived from the date span) so the Drift gate compares the durable store against an
    // INDEPENDENT derivation; negative `TotalSlack` survives as the constraint-infeasibility signal.
    static Option<ScheduleFloat> Resident(MpxjTask task) =>
        from es in Date(task.EarlyStart)
        from ls in Date(task.LateStart)
        select ScheduleFloat.Resident(es, Date(task.EarlyFinish).IfNone(es), ls, Date(task.LateFinish).IfNone(ls),
            Span(task.TotalSlack), Span(task.FreeSlack));

    static Option<ScheduleFormat> FormatOf(ProjectFile file) =>
        Optional(file.ProjectProperties?.FileType).Bind(static t => ScheduleFormat.Items.Find(f => string.Equals(f.Target.ToString(), t, StringComparison.OrdinalIgnoreCase)));

    static string Id(MpxjTask? task) => task?.ActivityID ?? task?.UniqueID?.ToString() ?? "";
    static Option<string> Opt(string? value) => Optional(value).Filter(static s => s.Length > 0);
    static Option<LocalDate> Date(DateTime? value) => Optional(value).Map(static d => LocalDate.FromDateTime(d));

    // MPXJ durations are unit-tagged (`Units` a `TimeUnit?`): read the magnitude through its unit into a NodaTime
    // `Duration` so a lag in hours/weeks is honored at its own unit rather than misread as days. The factor table
    // carries the work-time convention (8h day, 40h week, the elapsed units calendar-time) the durable store
    // re-derives in continuous days against; an untyped/absent duration is zero. A `Percent` unit (a P6
    // percent-of-duration lag) carries no absolute time and folds to zero here — a relative lag is the consumer's.
    static readonly FrozenDictionary<TimeUnit, double> UnitMinutes = new Dictionary<TimeUnit, double> {
        [TimeUnit.Minutes] = 1d, [TimeUnit.Hours] = 60d, [TimeUnit.Days] = 480d, [TimeUnit.Weeks] = 2400d, [TimeUnit.Months] = 9600d, [TimeUnit.Years] = 115_200d,
        [TimeUnit.ElapsedMinutes] = 1d, [TimeUnit.ElapsedHours] = 60d, [TimeUnit.ElapsedDays] = 1440d, [TimeUnit.ElapsedWeeks] = 10_080d, [TimeUnit.ElapsedMonths] = 43_200d, [TimeUnit.ElapsedYears] = 525_600d,
    }.ToFrozenDictionary();

    static Duration Span(MpxjDuration? duration) =>
        duration is { Units: { } unit } && UnitMinutes.TryGetValue(unit, out var perUnit)
            ? Duration.FromMinutes(duration.DurationValue * perUnit)
            : Duration.Zero;

    // The export inverse: rebuild a neutral `ProjectFile` the writer serializes — `AddTask` mints each activity,
    // the setters carry the canonical fields back through the BCL date/`MpxjDuration` boundary types, and one
    // `Relation.Builder` per edge re-creates the dependency network MPXJ writes out. The WBS hierarchy is flat
    // here (the writer re-derives outline level from `WBS`); a richer parent-child re-link is one column away.
    public static ProjectFile ToProjectFile(ScheduleNetwork network) {
        var file = new ProjectFile();
        network.Format.Iter(f => file.ProjectProperties.FileType = f.Target.ToString());
        var minted = network.Tasks.Fold(Map<string, MpxjTask>(), (acc, task) => {
            var node = file.AddTask();
            (node.Name, node.WBS) = (task.Name, task.WbsPath);
            node.Start = task.PlannedStart.ToDateTimeUnspecified();
            node.Finish = task.PlannedFinish.ToDateTimeUnspecified();
            node.Duration = MpxjDuration.GetInstance(task.PlannedDuration.Days, TimeUnit.Days);
            return acc.Add(task.TaskId, node);
        });
        network.Relations.Iter(edge => minted.Find(edge.Predecessor).Iter(pred => minted.Find(edge.Successor).Iter(succ =>
            new Relation.Builder(file).PredecessorTask(pred).SuccessorTask(succ).Type(edge.Kind.Mpxj).Lag(MpxjDuration.GetInstance(edge.Lag.Days, TimeUnit.Days)).Build())));
        return file;
    }
}

// The constraint-aware forward/backward CPM fold: adjacency derived ONCE, the four pass arms read the
// RelationKind float anchors and clamp to the ScheduleConstraint, the receipt reconciles the derived float
// against the scheduler's resident overlay.
public static class CpmPass {
    public static SchedulePass Resolve(ScheduleNetwork network, LocalDate dataDate) {
        var byId = network.ById;
        var preds = network.Predecessors;
        var succs = network.Successors;
        return Topological(byId.Keys.ToSeq(), network.Relations, succs).Match(
            Fail: _ => new SchedulePass(Map<string, ScheduleFloat>(), dataDate, dataDate, 0, 0, 0, 0, 0),
            Succ: order => {
                // A started/complete activity is pinned to its progress floor (progressed logic reads the actuals
                // against the data date); an unstarted activity floors at its planned start versus the data date
                // and a successor raises that floor by the modality-anchored predecessor date plus lag, then the
                // constraint clamps.
                var forward = order.Fold(Map<string, (LocalDate Es, LocalDate Ef)>(), (acc, id) => {
                    var task = byId[id];
                    var span = Span(task);
                    return acc.Add(id, task.ProgressFloor(dataDate).Match(
                        Some: progressed => progressed,
                        None: () => {
                            var floor = task.PlannedStart > dataDate ? task.PlannedStart : dataDate;
                            var drivenEs = preds.Find(id).Match(
                                Some: edges => edges.Map(e => e.Kind.DriveEarly(acc[e.Predecessor], span, e.Lag)).Fold(floor, static (m, d) => d > m ? d : m),
                                None: () => floor);
                            var es = ClampStart(task, drivenEs);
                            return (es, es.Plus(span));
                        }));
                });
                var finish = forward.Values.Map(static p => p.Ef).Fold(LocalDate.MinIsoValue, static (m, d) => d > m ? d : m);
                var backward = order.Rev().Fold(Map<string, (LocalDate Ls, LocalDate Lf)>(), (acc, id) => {
                    var task = byId[id];
                    var span = Span(task);
                    var drivenLf = succs.Find(id).Match(
                        Some: edges => edges.Map(e => e.Kind.DriveLate(acc[e.Successor], span, e.Lag)).Fold(finish, static (m, d) => d < m ? d : m),
                        None: () => finish);
                    var lf = ClampFinish(task, drivenLf, span);
                    return acc.Add(id, (lf.Minus(span), lf));
                });
                var floats = order.Fold(Map<string, ScheduleFloat>(), (acc, id) => {
                    var (es, ef) = forward[id];
                    var (ls, lf) = backward[id];
                    var free = succs.Find(id).Match(
                        Some: edges => edges.Map(e => e.Kind.FreeGap((es, ef), forward[e.Successor].Es, e.Lag)).Fold(int.MaxValue, Math.Min),
                        None: () => 0);
                    return acc.Add(id, ScheduleFloat.Window(es, ef, ls, lf, free == int.MaxValue ? 0 : int.Max(0, free)));
                });
                // The drift gate compares at WHOLE-DAY granularity: the scheduler stores slack as work-hours while
                // the re-derivation walks continuous calendar days, so an hours-vs-days `!=` would report
                // near-total spurious drift — the day-floor on both legs is the consistent calendar-blind gate.
                var (stored, drift) = byId.Values.Fold((Stored: 0, Drift: 0), (acc, t) => t.Resident.Match(
                    Some: r => (acc.Stored + 1, floats.Find(t.TaskId).Exists(d => d.TotalFloat.Days != r.TotalFloat.Days) ? acc.Drift + 1 : acc.Drift),
                    None: () => acc));
                return new SchedulePass(
                    floats, dataDate, finish,
                    floats.Values.Count(static f => f.Critical), floats.Values.Count(static f => f.Negative),
                    stored, floats.Count - stored, drift);
            });
    }

    // The resource leveling profile: each scheduled activity spreads its assigned `Units` across its
    // early-start..early-finish day span, the per-resource per-day demand sums, and the peak day per resource
    // projects the over-allocation surface — one fold over the float map and the task set, the `TaskResource`
    // rows made real rather than asserted.
    public static Seq<ResourceLoad> ResourceProfile(ScheduleNetwork network, SchedulePass pass) =>
        toSeq(network.Tasks.Bind(static t => t.Resources.Map(r => (Task: t, Resource: r))).GroupBy(static x => x.Resource.ResourceId)).Map(group => {
            var demand = group.Fold(Map<LocalDate, double>(), (acc, x) => pass.Floats.Find(x.Task.TaskId).Match(
                Some: f => toSeq(Range(0, int.Max(1, Period.DaysBetween(f.EarlyStart, f.EarlyFinish) + 1)))
                    .Fold(acc, (day, n) => day.AddOrUpdate(f.EarlyStart.PlusDays(n), u => u + x.Resource.Units, x.Resource.Units)),
                None: () => acc));
            var peak = demand.Fold((Units: 0d, Day: LocalDate.MinIsoValue), static (m, kv) => kv.Value > m.Units ? (kv.Value, kv.Key) : m);
            return new ResourceLoad(group.Key, peak.Units, peak.Day, group.Sum(static x => x.Resource.BudgetedCost));
        });

    // Task-level adapter onto the constraint-row clamp: present constraint clamps, absence passes through.
    static LocalDate ClampStart(ScheduleTask task, LocalDate es) =>
        task.Constraint.Match(Some: c => c.Kind.ClampEarly(c, es), None: () => es);
    static LocalDate ClampFinish(ScheduleTask task, LocalDate lf, Period span) =>
        task.Constraint.Match(Some: c => c.Kind.ClampLate(c, lf, span), None: () => lf);

    static Period Span(ScheduleTask task) => Period.FromDays(int.Max(0, task.PlannedDuration.Days));

    static Fin<Seq<string>> Topological(Seq<string> ids, Seq<TaskRelation> relations, Map<string, Seq<TaskRelation>> succs) {
        var degree = ids.Fold(Map<string, int>(), static (acc, id) => acc.Add(id, 0));
        var seeded = relations.Fold(degree, static (acc, e) => acc.AddOrUpdate(e.Successor, static n => n + 1, 1));
        return Drain(seeded.Filter(static (_, n) => n == 0).Keys.ToSeq(), seeded, succs, Seq<string>());

        static Fin<Seq<string>> Drain(Seq<string> ready, Map<string, int> degree, Map<string, Seq<TaskRelation>> succs, Seq<string> order) =>
            ready.HeadOrNone().Match(
                None: () => order.Count == degree.Count
                    ? FinSucc(order)
                    : FinFail<Seq<string>>(new ScheduleFault.CyclicNetwork($"unresolved:{degree.Count - order.Count}").ToError()),
                Some: id => {
                    var relaxed = succs.Find(id).IfNone(Seq<TaskRelation>()).Fold(degree, static (acc, e) => acc.AddOrUpdate(e.Successor, static n => n - 1, 0));
                    var unblocked = succs.Find(id).IfNone(Seq<TaskRelation>()).Map(static e => e.Successor).Filter(s => relaxed[s] == 0);
                    return Drain(ready.Tail.Concat(unblocked), relaxed, succs, order.Add(id));
                });
    }
}
```

| [INDEX] | [SURFACE]                       | [LAW]                                                                            |
| :-----: | :------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | `ScheduleImport.Read` `UniversalProjectReader.Read` auto-detect | one call admits ~20 schedule formats (P6 `XER`/`PMXML`, MS Project `MPP`/`MSPDI`/`MPX`, Asta, Phoenix, …); no extension branch, no hand-rolled parser |
|  [02]   | `ScheduleProject.Of` `ProjectFile` → `ScheduleNetwork` | the one boundary seam: `ChildTasks` WBS walk, `Predecessors` edges, `DateTime?`→`Option<LocalDate>`, unit-tagged `Duration`→NodaTime; the Java handle never threads out |
|  [03]   | resident overlay                | `ScheduleFloat.Resident` transcribes `Task.TotalSlack`/`FreeSlack` straight — the scheduler-solved float (never re-derived from the date span), the federation overlay |
|  [04]   | critical set                    | `SchedulePass.CriticalSet(floatDays)` = total-float-≤-threshold set; the float-zero floor at `0`, the near-critical band above, never the single longest task |
|  [05]   | float pass                      | `CpmPass.Resolve` forward/backward over derived adjacency; the drive is `RelationKind.DriveEarly`/`DriveLate` row behavior over the `(FromFinish, ToFinish)` anchors with SIGNED lag |
|  [06]   | constraint clamp                | `ScheduleConstraint.ClampEarly`/`ClampLate` `(Start, Sense)` row behavior; `Task.ConstraintType`/`ConstraintDate` clamp the early/late date; a HARD `On`/`Mandatory` pins both passes |
|  [07]   | negative float                  | `SchedulePass.NegativeFloat` = total-float-<-zero count; a hard start-pin earlier than the network drive forces late below early; never hidden by a non-negative clamp |
|  [08]   | reconciliation                  | `SchedulePass.Drift` = transcribed ≠ derived total-float count; the re-derivation gates the resident overlay, never replaces it; holds only because the resident leg is transcribed |
|  [09]   | schedule health                 | `SchedulePass.Cpli(baselineFinish)` DCMA index; the critical-path-length index gates schedule realism, a projection on the receipt |
|  [10]   | baseline erosion                | `ScheduleBaseline.Diff` → `Seq<FloatErosion>`; multi-baseline variance is a content-key float-erosion diff, never a table |
|  [11]   | retained/progressed             | `ScheduleTask.ProgressFloor(dataDate)` pins the forward pass; completed pins actuals, in-progress pins start + `PercentComplete`-remaining; load-bearing actuals, no new field |
|  [12]   | resource leveling               | `CpmPass.ResourceProfile` → `Seq<ResourceLoad>` peak demand off `Task.ResourceAssignments`; leveling and over-allocation are a real activity-network fold, never a prose claim |
|  [13]   | export leg                      | `ScheduleExport.Write` `UniversalProjectWriter(format.Target)`; the seven writable `FileFormat` members (`XER`/`PMXML`/`MSPDI`/`MPX`/`PLANNER`/`SDEF`/`JSON`), `MPP` unrepresentable |

## [03]-[TS_PROJECTION]

- Owner: `ScheduleFormatKind`, `RelationKindWire`, `ScheduleConstraintWire`, `TaskRelationWire`, `TaskConstraintWire`, `ScheduleTaskWire`, `ScheduleFloatWire`, `FloatErosionWire`, `ResourceLoadWire`, `SchedulePassWire`, `ScheduleBaselineWire` — the durable external-scheduler wire surface the TS-web Gantt, the CPM float overlay, and the resource-histogram decode; the 4D construction-state wire shapes (task-element link, per-element status, as-of state map) are the `Rasm.Bim/schedule` owner's projection the scrubber decodes from the Bim wire, never re-declared here.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface; a new task field, constraint, or float metric is one optional member on the existing interface.
- Boundary: dates cross as ISO-8601 date strings (NodaTime `LocalDate` round-trip); durations cross as the roundtrip-pattern string; the relation kind crosses as the two-letter code and reconstructs as the literal union; the activity constraint crosses as the canonical constraint key and the constraint date; the resident scheduler float overlay rides `ScheduleTaskWire.resident` so the Gantt distinguishes a scheduler-solved activity from an unscheduled one; the WBS path crosses as the ltree path string so the Gantt renders the hierarchy; the float, pass, resource-load, and baseline-erosion shapes cross primitive-mapped so the CPM overlay renders the threshold-critical set, the negative-float (`ScheduleFloatWire.negative` / `SchedulePassWire.negativeFloat`) infeasibility band, the per-resource over-allocation histogram (`ResourceLoadWire.peakUnits`/`peakDay`), and the slip set; the content-keyed baseline edition crosses as a 32-char lowercase-hex `UInt128` string (`edition.ToString("x32")`), never a `Uint8Array`, so the multi-baseline diff keys by string; the 4D construction-state wire (the task-element link, the per-element status, and the as-of state map) is the Bim schedule owner's projection — the scrubber decodes it from the Bim wire and federates the durable external float overlay to it by the IFC `GlobalId` the `ScheduleTaskWire.globalId` carries (the durable `TaskId` is the MPXJ activity code, never the federation key), so this surface carries only the durable external-scheduler shapes and never a 4D-state record.

```ts contract
type ScheduleFormatKind = "p6-xer" | "p6-pmxml" | "ms-project-mspdi" | "ms-project-mpx" | "planner" | "sdef" | "json";

type RelationKindWire = "FS" | "SS" | "FF" | "SF";

type ScheduleConstraintWire =
  | "start-on" | "start-no-earlier" | "start-no-later" | "finish-on" | "finish-no-later"
  | "mandatory-start" | "mandatory-finish" | "alap";

interface TaskRelationWire {
  predecessor: string;
  successor: string;
  kind: RelationKindWire;
  lag: string;
}

interface TaskConstraintWire {
  kind: ScheduleConstraintWire;
  on: string;
}

interface ScheduleFloatWire {
  earlyStart: string;
  earlyFinish: string;
  lateStart: string;
  lateFinish: string;
  totalFloat: string;
  freeFloat: string;
  critical: boolean;
  negative: boolean;
}

interface TaskResourceWire {
  resourceId: string;
  units: number;
  budgetedCost: number;
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
  resources: TaskResourceWire[];
  constraint: TaskConstraintWire | null;
  calendarId: string | null;
  resident: ScheduleFloatWire | null;
}

interface FloatErosionWire {
  taskId: string;
  wasTotalFloat: string;
  nowTotalFloat: string;
}

interface ResourceLoadWire {
  resourceId: string;
  peakUnits: number;
  peakDay: string;
  budgetedCost: number;
}

interface SchedulePassWire {
  floats: Record<string, ScheduleFloatWire>;
  dataDate: string;
  projectFinish: string;
  criticalLength: number;
  negativeFloat: number;
  stored: number;
  derived: number;
  drift: number;
}

interface ScheduleBaselineWire {
  name: string;
  edition: string;
  at: string;
}
```

## [04]-[RESEARCH]

- [MPXJ_INGRESS]: `MPXJ.Net.UniversalProjectReader.Read(Stream)` is the one auto-detecting ingress — it format-sniffs the input and dispatches to the right concrete `IProjectReader` (`PrimaveraXERFileReader`, `PrimaveraPMFileReader`, `MPPReader`, `MSPDIReader`, `MPXReader`, `AstaFileReader`, `PhoenixReader`, `GanttProjectReader`, `SureTrak*`, `SynchroReader`, …), so the lane reads ~20 scheduling-tool formats through one call with no extension branch and no hand-rolled tabular/XML parser; the neutral `ProjectFile` graph is the boundary type — `ChildTasks` is the WBS hierarchy root (`IList<Task>`), `Task.Predecessors`/`Successors` are the `IList<Relation>` dependency edges (`Relation.Type` a `RelationType?` = `FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`, `Relation.Lag` an MPXJ `Duration`), and `Task` carries the schedule fields the CPM pass and the resident overlay read (`Start`/`Finish`/`ActualStart`/`ActualFinish`/`Duration` as `DateTime?`/`Duration`, `PercentageComplete`, `Critical`, `EarlyStart`/`EarlyFinish`/`LateStart`/`LateFinish`, `TotalSlack`/`FreeSlack`, `ConstraintType`/`ConstraintDate`, `WBS`, `ActivityID`/`UniqueID`, `GUID`, `Calendar`, `ResourceAssignments`) — verified against the restored `MPXJ.Net` 16.4.1 surface; the IFC `GlobalId` federation correlation reads `Task.GUID` (`Option<string>.None` when the export carries no IFC counterpart) so the durable external float overlay federates to the host-neutral CPM/4D domain by `GlobalId` and never by the MPXJ activity `UniqueID`/`ActivityID`.
- [RESIDENT_OVERLAY_VS_DERIVATION]: the durable external-scheduler store TRANSCRIBES MPXJ's parsed `Task.TotalSlack`/`FreeSlack` as the federation overlay (`ScheduleFloat.Resident`, never re-derived from the date columns) AND re-derives an independent `ScheduleFloat` through `CpmPass.Resolve` over the `TaskRelation` DAG (`ScheduleFloat.Window`) — the forward pass derives early start as the maximum over predecessors of the `RelationKind`-anchored predecessor date (`FromFinish` reads the predecessor finish, else its start) plus the SIGNED edge lag (a negative `Relation.Lag` is a lead the drive preserves at whole-day resolution), the backward pass the symmetric late finish, both CLAMPED to the `ScheduleConstraint` `(Start, Sense)` row behavior (`NoLater` caps, `NoEarlier` floors, `On`/`Mandatory` hard-pin both passes, `ALAP` defers) and the data date — a hard start-pin (`On`/`Mandatory`) earlier than the predecessor drive forces the late date below the early date, the source of negative total float `Period.DaysBetween(EarlyStart, LateStart)` the `NegativeFloat` count surfaces, free float the per-modality `RelationKind.FreeGap` successor gap less the edge lag — and the `SchedulePass.Drift` count reconciles the derived total float against the transcribed overlay so a re-sequenced plan whose stored float is stale surfaces as drift rather than silently trusting either source, a reconciliation that is only non-trivial because the resident leg reads the parsed slack rather than re-deriving the same span the derivation walks; the adjacency is derived once into `Predecessors`/`Successors` maps and `Topological` runs Kahn over it, a residual cycle aborting `ScheduleFault.CyclicNetwork`; the calendar-blind day arithmetic is the deliberate split from the Bim `WorkCalendar.Advance` working-time fold — the durable store reads the scheduler's already-calendar-resolved dates and re-derives in continuous days as the drift gate, while the host-neutral Bim owner owns the IFC work-calendar CPM.
- [DURATION_UNIT_LIFT]: the MPXJ `Duration` is unit-tagged (`DurationValue` plus `Units` a `TimeUnit?` = `Days`/`Hours`/`Weeks`/`Months`/…) so the boundary reads a lag/duration through its unit via `Duration.ConvertUnits(value, fromUnits, TimeUnit.Days, …)` rather than assuming days — a P6 XER stores durations in hours while an MS-Project export may use days, and MPXJ exposes the native unit so the canonical NodaTime `Duration` is unit-correct at one seam; the data date sources from `ProjectProperties.StatusDate` (the schedule's as-of date), the project bounds from `ProjectProperties.StartDate`/`FinishDate`, and the forward-vs-backward scheduling stance from `ProjectProperties.ScheduleFrom` (`Start`/`Finish`); the export leg writes the seven `FileFormat` members through `UniversalProjectWriter` and there is NO `MPP` writer (a Microsoft binary format MPXJ reads but cannot write), so the `ScheduleFormat` axis carries exactly the writable set and an `MPP` round-trip exports to `MSPDI`/`MPX` instead.
