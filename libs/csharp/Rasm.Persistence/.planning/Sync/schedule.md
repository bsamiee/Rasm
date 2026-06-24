# [PERSISTENCE_SCHEDULE_INTERCHANGE]

Rasm.Persistence owns the durable external-scheduler store and its sync — the Primavera P6 XER and Microsoft Project XML residence plus its CPM float algebra over the external scheduler's relation DAG. `ScheduleImport` admits a P6 XER (`%T`/`%F`/`%R`-delimited multi-table text) and an MS-Project MSPDI XML into a typed `ScheduleTask` activity network carrying P6's RESIDENT computed overlay — `ScheduleTask` reads `early_start_date`/`late_start_date`/`total_float_hr_cnt`/`free_float_hr_cnt` straight off the `TASK` table into the typed `ScheduleFloat` the source scheduler already solved, plus the `cstr_type`/`cstr_date` activity constraint and the `clndr_id` calendar correlation, so the durable store RESIDES the external float overlay rather than re-deriving it; `CpmPass.Resolve` runs the forward/backward float pass only as the INDEPENDENT re-derivation that gates the resident overlay — early/late start-finish and total/free float over the `TaskRelation` DAG honoring the `ScheduleConstraint` clamps — so the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor and the near-critical band), a constraint-pressured plan surfaces its negative-float set rather than hiding it under a non-negative clamp, a schedule slip is a typed `FloatErosion` delta against a `ScheduleBaseline`, the resource-loading columns drive the `CpmPass.ResourceProfile` over-allocation fold, and the `SchedulePass` receipt carries the data date, the project finish, the critical-path and negative-float counts, the `Cpli` DCMA schedule-health index, and the stored-versus-derived float reconciliation that only holds because the resident overlay TRANSCRIBES P6's `total_float_hr_cnt` rather than re-deriving it. `ScheduleBaseline` is one content-addressed snapshot so a baseline-versus-current variance is a multi-baseline content-key diff; the CPM walk is one DAG algebra serving CPM, lineage, and the commit-DAG. The 4D construction-state domain semantics — the schedule-activity-to-`ElementSet` binding, the per-element `FourDStatus`, the as-of construction-state fold, and the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — are the AEC-domain owner `Rasm.Bim/schedule`'s concern, consumed here only at the `Sync/schedule ⇄ Rasm.Bim/schedule # [WIRE]: P6/MS-Project + 4D construction domain` wire; per the `[SCHEDULE_NETWORK_DEPTH]` boundary the source lanes split — Persistence owns the durable P6/MS-Project store plus its `CpmPass` over the external `TaskRelation` DAG, Bim owns the host-neutral CPM/4D fold over the IFC-projected `SequenceRel` network — strata-legal as app-platform consuming aec-domain. The two stores federate by the IFC `GlobalId`: a `ScheduleTask` carries the `Option<string> GlobalId` correlation to the Bim `ConstructionTask.GlobalId` (the IFC process the external P6/MS-Project activity maps to, `None` for an activity with no IFC counterpart), and the wire join keys the durable external float overlay to the host-neutral CPM/4D domain by that `GlobalId` — the external `TaskId` is the P6/MS-Project activity code, never the federation key, so a Bim `CriticalPath` reading a P6 relation or a Persistence `CpmPass` re-deriving the IFC `SequenceRel` network is the cross-package drift the relocation forbids. The Sep tabular reader (`Query/lanes#ANALYTICAL_LANE`), the cross-document link (`Query/federation#CROSS_DOC_LINKS`), the element-set currency (`Query/federation#ELEMENT_SET_ALGEBRA`), the time-travel AS-OF fold (`Version/timetravel#TIME_TRAVEL`), and `ClockPolicy`/`ReceiptSinkPort`/`CorrelationId` arrive settled. The Compute P6/XER parse companion produces the canonical activity bytes this store ingests.

## [01]-[INDEX]

- [01]-[SCHEDULE_STORE]: P6 XER and MS-Project XML admission; typed activity network carrying P6's transcribed resident float overlay, activity constraints, and calendar correlation; WBS; constraint-aware CPM float algebra with negative-float and threshold-critical sets, resource-leveling over-allocation, a typed `SchedulePass` receipt with the `Cpli` DCMA index, and a baseline `FloatErosion` diff.
- [02]-[TS_PROJECTION]: schedule task, relation, float, constraint, and baseline wire shapes.

## [02]-[SCHEDULE_STORE]

- Owner: `ScheduleFormat` the interchange-format axis (`p6-xer` tab-table text, `ms-project-xml` MSPDI document) carrying the `bool Tabular` ingest-lane discriminant; `RelationKind` the `[SmartEnum<string>]` dependency-modality axis under the IFC two-letter key carrying the P6 `pred_type` alias, the `(bool FromFinish, bool ToFinish)` float-algebra anchors, and the `DriveEarly`/`DriveLate`/`FreeGap` row behavior the forward/backward/free-float passes dispatch (signed lag, a lead preserved); `ScheduleConstraint` the `[SmartEnum<string>]` P6 `cstr_type` activity-constraint axis carrying the `(bool Start, ClampSense Sense)` policy and the `ClampEarly`/`ClampLate` row behavior the pass dispatches (a `Hard` `On`/`Mandatory` pinning both passes); `ScheduleTask` the typed activity record carrying the resource-loading columns, the P6-resident `Option<ScheduleFloat>` overlay, the `Option<TaskConstraint>` constraint, and the `Option<string>` calendar id; `TaskRelation` the predecessor/successor dependency carrying the lag as a `Duration`; `ScheduleFloat` the typed per-activity early/late start-finish + total/free float window carrying the `Critical` (float-zero) and `Negative` (constraint-infeasible) predicates plus the `Resident` (transcribe stored `*_float_hr_cnt`) and `Window` (derive from the date span) admission factories; `FloatErosion` the per-activity baseline-versus-current total-float delta; `ResourceLoad` the per-resource peak-unit/peak-day/budgeted-cost leveling profile carrying the `Overallocated(capacity)` predicate; `ScheduleBaseline` the content-addressed baseline-snapshot axis; `SchedulePass` the typed CPM receipt carrying the per-task `ScheduleFloat`, the data date, the project finish, the critical-path and negative-float counts, and the stored-versus-derived reconciliation, projecting the `CriticalSet(floatDays)` threshold critical set and the `Cpli(baselineFinish)` DCMA schedule-health index; `ScheduleImport` the static admission surface owning the XER per-section `SepReader` projection (header indices resolved once through `Header.ColNames`), the MSPDI `XDocument` projection, the WBS ltree fold, and the `ScheduleBaseline.Diff` erosion projection; `CpmPass` the static forward/backward DAG-walk fold owning the constraint-aware float pass and the `ResourceProfile` leveling fold.
- Cases: `ScheduleFormat` `P6Xer` (tabular) | `MsProjectXml` (document); `RelationKind` `FinishToStart` (`FS`/`PR_FS`) | `StartToStart` (`SS`/`PR_SS`) | `FinishToFinish` (`FF`/`PR_FF`) | `StartToFinish` (`SF`/`PR_SF`), each row carrying the float anchors the four pass arms read; `ScheduleConstraint` `StartOn` (`CS_MSO`) | `StartNoEarlierThan` (`CS_MSOA`) | `StartNoLaterThan` (`CS_MSOB`) | `FinishOn` (`CS_MEO`) | `FinishNoLaterThan` (`CS_MEOB`) | `MandatoryStart` (`CS_MANDSTART`) | `MandatoryFinish` (`CS_MANDFIN`) | `AsLateAsPossible` (`CS_ALAP`); a task carries id, WBS path, name, planned/actual start and finish, planned duration, percent-complete, the resource-id/units/budgeted-cost loading columns, the resident `ScheduleFloat` overlay P6 solved, the activity constraint, and the calendar id; `ScheduleFloat` carries the early/late start-finish and total/free float, the critical activity being the total-float-zero floor and the negative-float activity the constraint-infeasible set; `SchedulePass` carries the float map, the data date, the project finish, the critical-path and negative-float counts, and the `Stored`/`Derived`/`Drift` reconciliation counts; `ResourceLoad` carries the per-resource peak concurrent units, the peak day, and the rolled budgeted cost.
- Entry: `public static Fin<ScheduleNetwork> ReadXer(Stream xer)` admits the P6 XER `TASK`/`TASKPRED`/`PROJWBS`/`CALENDAR` sections — `XerTables.Sift` splits the `%T`/`%F`/`%R` markers into per-table Sep-ready blocks, each read through one `SepReader` whose header indices resolve once into the typed-row projection, aborting `ScheduleFault.MalformedTable` on an absent required column or a relation naming an undeclared activity; `public static Fin<ScheduleNetwork> ReadMsProject(Stream xml)` admits the MSPDI document through one `XDocument` load projecting the `Project/Tasks/Task` and `PredecessorLink` elements onto the same `ScheduleNetwork`, mapping the MS-Project `Type` integer onto the `RelationKind` and the `ConstraintType` integer onto the `ScheduleConstraint`; `public static SchedulePass CpmPass.Resolve(ScheduleNetwork network, LocalDate dataDate)` runs the constraint-aware forward/backward pass producing the typed receipt; `public Seq<string> SchedulePass.CriticalSet(int floatDays)` projects the float-threshold critical set (the total-float-zero floor at `0`, the near-critical band above) and `public double SchedulePass.Cpli(LocalDate baselineFinish)` the DCMA critical-path-length index; `public static Seq<ResourceLoad> CpmPass.ResourceProfile(ScheduleNetwork network, SchedulePass pass)` folds the resource-loading columns into the per-resource peak-demand leveling profile; `public static ScheduleBaseline ScheduleBaseline.Of(string name, ScheduleNetwork network, Instant at)` mints the content-addressed baseline and `ScheduleBaseline.Diff(SchedulePass baseline, SchedulePass current)` projects the `Seq<FloatErosion>` slip set.
- Auto: the P6 XER is an `%T`-table/`%F`-field/`%R`-row multi-table text format so `XerTables.Sift` reads it once through Sep with the tab separator and the per-table `%F` header row — the `TASK` table projects activities (reading `task_code`/`wbs_id`/`task_name`/`target_start_date`/`target_end_date`/`act_start_date`/`act_end_date`/`target_drtn_hr_cnt`/`phys_complete_pct`/`clndr_id`/`cstr_type`/`cstr_date` plus P6's RESIDENT `early_start_date`/`early_end_date`/`late_start_date`/`late_end_date`/`total_float_hr_cnt`/`free_float_hr_cnt` overlay columns), `TASKPRED` projects relations (`pred_task_id`/`task_id`/`pred_type`/`lag_hr_cnt`), `PROJWBS` projects the WBS hierarchy as an ltree path, and `CALENDAR` projects the `clndr_id` correlation, so no per-format schedule library enters and the durable store RESIDES P6's solved float rather than re-deriving it; the MS-Project MSPDI XML loads once through a BCL `XDocument` over the `Project`/`Tasks`/`Task`/`PredecessorLink` element shape (STJ reads JSON, never XML — the document lane is `System.Xml.Linq`) sharing the one `ScheduleNetwork` projection; the activity network is a DAG keyed on task id with `TaskRelation` edges so the CPM float pass is one DAG-walk the commit-DAG and lineage-DAG share — `CpmPass.Resolve` derives the `Predecessors`/`Successors` adjacency once, `Topological` runs Kahn over it (a residual cycle aborting `ScheduleFault.CyclicNetwork`), the forward pass folds each activity in topological order deriving early start as the maximum over predecessors of the modality-anchored predecessor date plus the SIGNED edge lag (a negative lag is a P6 lead — `RelationKind.DriveEarly` carries the sign, never floors it to zero) then CLAMPED to the `ScheduleConstraint`, the backward pass folds in reverse deriving late finish where a HARD start constraint (`On`/`Mandatory`) pins the late finish below the network drive so the constraint produces the negative float the `NegativeFloat` count surfaces, total float is `Period.DaysBetween(EarlyStart, LateStart)` and free float the per-modality `RelationKind.FreeGap` (successor early-start minus the driving finish/start minus the edge lag, floored at zero), all dispatched through the `RelationKind` and `ScheduleConstraint` row behavior rather than per-arm `Switch` bodies; the `SchedulePass` receipt reconciles each derived `ScheduleFloat` against the task's resident overlay so `Drift` counts the activities where the durable store's TRANSCRIBED `total_float_hr_cnt` and the re-derivation disagree (a re-sequenced plan whose stored float is stale); the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor at `0`, the near-critical band above) rather than the single longest task; the WBS hierarchy is an ltree path so a roll-up-by-WBS rides the ltree operators; the resource-loading columns drive `CpmPass.ResourceProfile`, the per-resource leveling fold spreading each activity's `ResourceUnits` across its early-start..early-finish span into the peak-day over-allocation surface; the imported schedule is content-addressed so a re-imported revision dedupes and a `ScheduleBaseline.Of` snapshot is one content key, a baseline-versus-current variance being a `ScheduleBaseline.Diff` `FloatErosion` set keyed on the multi-baseline content key rather than a per-baseline table.
- Receipt: an import rides `store.schedule.import` carrying the format, the task count, the relation count, and the resident-overlay-present count; the typed `SchedulePass(Floats, DataDate, ProjectFinish, CriticalLength, NegativeFloat, Stored, Derived, Drift)` is the CPM evidence carrying the per-task `ScheduleFloat`, the data date, the project finish `LocalDate`, the critical-path and negative-float (constraint-infeasibility) counts, and the stored-versus-derived reconciliation; a baseline diff rides `store.schedule.baseline` carrying the `FloatErosion` slip count; a resource-leveling fold rides `store.schedule.resource` carrying the `ResourceLoad` over-allocation peaks; a WBS fold rides `store.schedule.wbs` — never a generic `IReceipt`.
- Packages: Sep, System.IO.Hashing, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox (`System.Xml.Linq` MSPDI document lane).
- Growth: a new interchange format is one `ScheduleFormat` row plus its reader; a new task field is one column on `ScheduleTask` (the resource-loading, resident-overlay, constraint, and calendar columns are exactly this — columns on the existing record, never a second resource/float/constraint record); a new relation type is one `RelationKind` row carrying its float anchors and breaking nothing because the four pass arms read the anchor rows; a new activity constraint is one `ScheduleConstraint` row carrying its clamp policy the pass already reads; a new float metric is one column on `ScheduleFloat` plus its predicate; a new CPM-evidence fact is one field on `SchedulePass` and a new schedule-health index one projection on it (the `CriticalSet` threshold set and the `Cpli` DCMA index are exactly this — projections on the existing receipt, never a parallel metrics record); a new leveling axis is one column on `ResourceLoad`; a new baseline is one `ScheduleBaseline.Of` content key, never a per-baseline table; zero new surface — a per-format schedule model, a Primavera SDK wrapper, an MS-Project COM interop, a separate stored-float table, a parallel resource-leveling record, or a per-metric critical-path calculator is the deleted form because the XER rides the Sep tabular reader, the MSPDI rides `XDocument`, both project the one `ScheduleNetwork`, and CPM is one DAG algebra serving CPM, lineage, and the commit-DAG.
- Boundary: the P6 XER rides the Sep tabular reader so the import is a tabular read, never a Primavera SDK or a per-vendor schedule library — the XER is a tab-separated multi-table file whose `%T`/`%F`/`%R`/`%E` markers delimit `TASK`/`TASKPRED`/`PROJWBS`/`CALENDAR` sections, and each section reads through one `SepReader` whose `Header.ColNames` resolve the column indices once (`Query/lanes#ANALYTICAL_LANE`) so the row read is by name so a positional ordinal parse is the deleted form AND a caller-supplied projection delegate is the deleted form — the owner OWNS the column projection in-fence; the MS-Project MSPDI XML rides a BCL `System.Xml.Linq.XDocument` load (STJ reads JSON, never XML — claiming STJ source generation over an XML document is the corrected illusion) so a COM interop or an Office library is the deleted form; both formats project the one `ScheduleNetwork` so a per-format model is the deleted form; the durable store TRANSCRIBES P6's computed `total_float_hr_cnt`/`free_float_hr_cnt` straight off the `TASK` columns through `ScheduleFloat.Resident` (never re-derived from the date span) as the federation float the Bim `CriticalPath` overlays by `GlobalId` — discarding the stored overlay and re-deriving on every read is the deleted form, and recomputing the resident total float from `early_start_date`/`late_start_date` instead of reading `total_float_hr_cnt` is the corrected illusion that collapsed the `Drift` gate into a tautology, so `CpmPass.Resolve` re-derivation through `ScheduleFloat.Window` reconciles against the transcribed overlay as a genuine `Drift` check, never as the sole truth; the activity network is a DAG so the critical-path, total-float, and free-float computations are one `CpmPass` forward/backward DAG walk over the relation edges — the float pass reads the `RelationKind` `(FromFinish, ToFinish)` anchor rows and the `ScheduleConstraint` clamp rows (a statement-switch, an if/else ladder over the four arms, OR an unclamped pass that ignores `cstr_type`/`cstr_date` is the deleted form), and the critical path is the float-threshold `SchedulePass.CriticalSet` (the float-zero floor at `0`, the near-critical band above), never the single longest-duration task the prior stub returned, with the negative-float count surfacing the constraint-infeasible set a naive non-negative clamp would have hidden; the adjacency is derived ONCE into `Predecessors`/`Successors` maps so the prior `byId.Values.Filter(...)` rescan per node in `Forward`/`Backward`/`FloatOf`/`Drain` is the deleted O(V·E) form; `ScheduleFloat` is a typed record struct carrying the float window, never a loose tuple, and a schedule slip is a typed `FloatErosion` against a baseline `SchedulePass`; the resource-loading columns (`ResourceId`/`ResourceUnits`/`BudgetedCost`) ride the existing record and drive the real `CpmPass.ResourceProfile` leveling fold into the typed `ResourceLoad` peak-demand surface, never a second resource record or a prose-only claim; `ScheduleBaseline` rides the existing content-addressed snapshot identity (`XxHash128.HashToUInt128` over the sorted activity-edition set) so a baseline-versus-current variance is a multi-baseline content-key diff and a per-baseline table is the deleted form; retained/progressed-logic scheduling is the `ScheduleTask.ProgressFloor(dataDate)` behavior the forward pass dispatches — a completed activity pins both legs to its actuals, an in-progress activity pins its start to `ActualStart` and its finish to the data date plus the `PercentComplete`-remaining span, an unstarted activity defers to the network drive — so `ActualStart`/`ActualFinish`/`PercentComplete` are load-bearing pass inputs, never dead status columns, with no new field; dates ride NodaTime `LocalDate`/`Period`/`Duration` so a schedule date never becomes a `DateTime` sentinel; the WBS is an ltree path so a cost-by-WBS rollup (`Store/profiles#COST_ROLLUP`) joins the schedule WBS to the takeoff classification through one ltree join; the Compute P6/XER parse companion (`Rasm.Compute/Runtime/codecs#FIELD_RESULT_CODEC` `InterchangeIo` tabular ingest, the same `Query/pipeline ⇄ Rasm.Compute/Runtime/codecs # [PORT]: parse-to-canonical-bytes (Extract)` seam the bulk pipeline reads) produces the canonical activity bytes this store ingests — Compute owns the parse, Persistence owns the durable residence and the CPM float algebra.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.Text;
using System.Xml.Linq;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using NodaTime.Text;
using nietras.SeparatedValues;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Persistence;

// --- [TYPES] ------------------------------------------------------------------------------
public sealed class ScheduleKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class ScheduleFormat {
    public static readonly ScheduleFormat P6Xer = new("p6-xer", tabular: true);
    public static readonly ScheduleFormat MsProjectXml = new("ms-project-xml", tabular: false);

    public bool Tabular { get; }
}

// The dependency-modality axis: one row per IFC two-letter key carrying the P6 `pred_type`
// alias and the (FromFinish, ToFinish) float-algebra anchors the four CPM pass arms read, so a
// new modality is a row carrying its anchors, never a fifth Switch body.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class RelationKind {
    public static readonly RelationKind FinishToStart = new("FS", p6: "PR_FS", fromFinish: true,  toFinish: false);
    public static readonly RelationKind StartToStart  = new("SS", p6: "PR_SS", fromFinish: false, toFinish: false);
    public static readonly RelationKind FinishToFinish = new("FF", p6: "PR_FF", fromFinish: true,  toFinish: true);
    public static readonly RelationKind StartToFinish  = new("SF", p6: "PR_SF", fromFinish: false, toFinish: true);

    public string P6 { get; }
    public bool FromFinish { get; }
    public bool ToFinish { get; }

    private static readonly Lazy<FrozenDictionary<string, RelationKind>> ByP6 =
        new(static () => Items.ToFrozenDictionary(static r => r.P6, StringComparer.Ordinal));

    public static RelationKind OfP6(string code) => ByP6.Value.GetValueOrDefault(code.Trim(), FinishToStart);

    // MSPDI PredecessorLink.Type: 0=FF 1=FS 2=SF 3=SS.
    public static RelationKind OfMsProject(int type) => type switch {
        0 => FinishToFinish, 2 => StartToFinish, 3 => StartToStart, _ => FinishToStart,
    };
}

// The modality drive is row behavior, not a free Switch: each anchor pair maps the predecessor's
// driving date plus the SIGNED lag onto the successor's early/late constraint. P6 lag is a Duration —
// a negative lag is a lead (FS-2d starts the successor two days before the predecessor finishes), so the
// drive carries the sign at whole-day resolution rather than flooring it to zero.
extension(RelationKind kind) {
    Period AsLag(Duration lag) => Period.FromDays(lag.Days);

    // Forward: the predecessor's anchor (its finish when the edge leaves a finish, else its start) plus
    // the signed lag; the successor START backs off one task span when the edge targets a finish.
    public LocalDate DriveEarly((LocalDate Es, LocalDate Ef) pred, Period span, Duration lag) {
        var from = (kind.FromFinish ? pred.Ef : pred.Es).Plus(kind.AsLag(lag));
        return kind.ToFinish ? from.Minus(span) : from;
    }

    // Backward: the symmetric pull — the successor's anchor minus the signed lag; the predecessor LATE
    // start advances one span when the edge leaves a start (a start-anchored edge frees the late finish).
    public LocalDate DriveLate((LocalDate Ls, LocalDate Lf) succ, Period span, Duration lag) {
        var to = (kind.ToFinish ? succ.Lf : succ.Ls).Minus(kind.AsLag(lag));
        return kind.FromFinish ? to : to.Plus(span);
    }

    // Free float against ONE successor edge: the successor's earliest start minus this activity's driving
    // finish minus the edge lag — the slack before this activity disturbs that successor on this modality.
    public int FreeGap((LocalDate Es, LocalDate Ef) self, LocalDate successorEarlyStart, Duration lag) =>
        Period.DaysBetween(kind.ToFinish ? self.Ef : self.Es, successorEarlyStart) - lag.Days;
}

// The P6 `cstr_type` activity-constraint axis: one row per constraint carrying the (Start, Sense)
// clamp policy the forward/backward pass honors, so the pass discriminates the clamp by row data
// rather than a constraint Switch body.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ScheduleKeyPolicy, string>]
public sealed partial class ScheduleConstraint {
    public static readonly ScheduleConstraint StartOn            = new("CS_MSO",       start: true,  sense: ClampSense.On);
    public static readonly ScheduleConstraint StartNoEarlierThan = new("CS_MSOA",      start: true,  sense: ClampSense.NoEarlier);
    public static readonly ScheduleConstraint StartNoLaterThan   = new("CS_MSOB",      start: true,  sense: ClampSense.NoLater);
    public static readonly ScheduleConstraint FinishOn           = new("CS_MEO",       start: false, sense: ClampSense.On);
    public static readonly ScheduleConstraint FinishNoLaterThan  = new("CS_MEOB",      start: false, sense: ClampSense.NoLater);
    public static readonly ScheduleConstraint MandatoryStart     = new("CS_MANDSTART", start: true,  sense: ClampSense.Mandatory);
    public static readonly ScheduleConstraint MandatoryFinish    = new("CS_MANDFIN",   start: false, sense: ClampSense.Mandatory);
    public static readonly ScheduleConstraint AsLateAsPossible   = new("CS_ALAP",      start: false, sense: ClampSense.Latest);

    public bool Start { get; }
    public ClampSense Sense { get; }

    public static Option<ScheduleConstraint> OfP6(string code) =>
        TryGet(code.Trim(), out var c) ? Optional(c) : None;

    // MSPDI Task.ConstraintType: 0=ASAP 1=ALAP 2=MSO 3=MFO 4=SNET 5=SNLT 6=FNET 7=FNLT.
    public static Option<ScheduleConstraint> OfMsProject(int type) => type switch {
        1 => Some(AsLateAsPossible), 2 => Some(StartOn), 3 => Some(FinishOn),
        4 => Some(StartNoEarlierThan), 5 => Some(StartNoLaterThan), 7 => Some(FinishNoLaterThan), _ => None,
    };
}

// The clamp is row behavior keyed on (Start, Sense), so the pass never switches on the constraint kind.
// A hard pin (On/Mandatory) forces the date in BOTH passes — the only way the network produces negative
// float: a mandatory start earlier than the predecessor drive pulls the late date below the early date.
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

    // A finish-side constraint clamps the backward late finish; a HARD start-side constraint also pins the
    // late finish to (start + span) so a mandatory/on start floors the late date and surfaces negative float.
    public LocalDate ClampLate(TaskConstraint c, LocalDate lf, Period span) =>
        constraint.Start
            ? constraint.Hard ? c.On.Plus(span) : lf
            : constraint.Apply(c.On, lf);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct TaskConstraint(ScheduleConstraint Kind, LocalDate On);

public sealed record TaskRelation(string Predecessor, string Successor, RelationKind Kind, Duration Lag);

// P6's RESIDENT solved overlay, admitted off the TASK table — the federation float the Bim
// CriticalPath overlays by GlobalId. CpmPass.Resolve re-derives an INDEPENDENT ScheduleFloat and
// reconciles against this stored one; this is never discarded for the re-derivation. The two factories
// keep the regimes distinct: Resident TRANSCRIBES P6's stored total_float_hr_cnt/free_float_hr_cnt
// (negative float survives — a constraint-pressured plan carries it), Window DERIVES total float from
// the late-minus-early date span. Negative float marks an infeasible plan; Critical is the float-zero
// floor INCLUDING the negative set so a constraint slip stays on the critical path.
public readonly record struct ScheduleFloat(
    LocalDate EarlyStart, LocalDate EarlyFinish,
    LocalDate LateStart, LocalDate LateFinish,
    Duration TotalFloat, Duration FreeFloat) {
    public bool Critical => TotalFloat <= Duration.Zero;
    public bool Negative => TotalFloat < Duration.Zero;

    public static ScheduleFloat Resident(LocalDate es, LocalDate ef, LocalDate ls, LocalDate lf, double totalHours, double freeHours) =>
        new(es, ef, ls, lf, Duration.FromHours(totalHours), Duration.FromHours(double.Max(0d, freeHours)));

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
    string ResourceId,
    double ResourceUnits,
    double BudgetedCost,
    Option<TaskConstraint> Constraint,
    Option<string> CalendarId,
    Option<ScheduleFloat> Resident,
    UInt128 ScheduleEdition) {
    // Progressed-logic data-date floor: a completed activity (ActualFinish present) pins both legs to its
    // actuals; an in-progress activity (ActualStart present, not complete) pins its start to the actual and
    // its finish to the data date plus the percent-complete REMAINING span; an unstarted activity defers to
    // the network drive (None). This is what makes ActualStart/ActualFinish/PercentComplete load-bearing in
    // the pass rather than dead status columns — without it the forward pass ignores progress entirely.
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

// One activity-network record: the task set plus the dependency-edge set, the durable owner the
// CPM pass, the baseline, and the federation read by reference.
public sealed record ScheduleNetwork(ScheduleFormat Format, Seq<ScheduleTask> Tasks, Seq<TaskRelation> Relations) {
    public Map<string, ScheduleTask> ById => Tasks.ToMap(static t => t.TaskId);
    public Map<string, Seq<TaskRelation>> Predecessors => Relations.GroupBy(static r => r.Successor).ToMap(static g => g.Key, static g => g.ToSeq());
    public Map<string, Seq<TaskRelation>> Successors => Relations.GroupBy(static r => r.Predecessor).ToMap(static g => g.Key, static g => g.ToSeq());
}

public readonly record struct FloatErosion(string TaskId, Duration WasTotalFloat, Duration NowTotalFloat) {
    public Duration Slip => WasTotalFloat - NowTotalFloat;
    public bool Eroded => Slip > Duration.Zero;
}

// The per-resource leveling profile: the concurrent unit demand summed across the scheduled span and
// the peak day, the over-allocation surface a resource-leveled schedule reads against the resource's
// declared capacity — the activity-network fold the resource-loading columns make real, never a
// second resource record.
public readonly record struct ResourceLoad(string ResourceId, double PeakUnits, LocalDate PeakDay, double BudgetedCost) {
    public bool Overallocated(double capacity) => PeakUnits > capacity;
}

// The typed CPM receipt: the per-task derived float, the project bounds, the critical-path
// length, the negative-float (constraint-infeasibility) count, the stored-versus-derived
// reconciliation, and the DCMA schedule-health index set — never a generic IReceipt. The metric
// projections fold the float map ONCE rather than minting a parallel metrics record.
public sealed record SchedulePass(
    Map<string, ScheduleFloat> Floats, LocalDate DataDate, LocalDate ProjectFinish,
    int CriticalLength, int NegativeFloat, int Stored, int Derived, int Drift) {
    // DCMA Critical Path Length Index: the planned critical-path length plus the project finish slip
    // over the planned length — 1.0 on track, above 1.0 behind. A zero planned span reads as on track.
    public double Cpli(LocalDate baselineFinish) =>
        Period.DaysBetween(DataDate, baselineFinish) is var planned and not 0
            ? (planned + Period.DaysBetween(baselineFinish, ProjectFinish)) / (double)planned : 1d;

    // The float-threshold critical set: the float-zero floor at threshold 0, the near-critical band
    // for a positive day threshold — one polymorphic projection the Gantt overlay drives by slider.
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
    public static ScheduleFault Create(string message) => new MalformedTable(message);
    public sealed record MalformedTable : ScheduleFault { public MalformedTable(string detail) : base(detail, 7301) { } }
    public sealed record DanglingEdge : ScheduleFault { public DanglingEdge(string detail) : base(detail, 7302) { } }
    public sealed record CyclicNetwork : ScheduleFault { public CyclicNetwork(string detail) : base(detail, 7303) { } }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ScheduleImport {
    public static Fin<ScheduleNetwork> ReadXer(Stream xer) => XerTables.Sift(xer);

    public static Fin<ScheduleNetwork> ReadMsProject(Stream xml) =>
        Try.lift(() => XDocument.Load(xml)).Run()
            .MapFail(static e => new ScheduleFault.MalformedTable($"mspdi-unreadable:{e.Message}").ToError())
            .Bind(Mspdi.Project);

    public static Fin<ScheduleNetwork> Admit(ScheduleFormat format, Stream source) =>
        format.Tabular ? ReadXer(source) : ReadMsProject(source);
}

// XER admission: the %T/%F/%R/%E envelope holds many tables, so it splits at the markers once into
// per-table Sep-ready blocks (the %F header line plus its %R data lines), and each block reads
// through one SepReader by header-named column. The owner OWNS the projection; no caller delegate.
public static class XerTables {
    static readonly SepReaderOptions Options = Sep.New('\t').Reader(o => o with { Unescape = false });

    public static Fin<ScheduleNetwork> Sift(Stream xer) {
        using var reader = new StreamReader(xer, Encoding.UTF8);
        var blocks = Split(reader);
        return Tasks(blocks).Bind(tasks => Edges(blocks, toHashSet(tasks.Map(static t => t.TaskId)))
            .Map(edges => new ScheduleNetwork(ScheduleFormat.P6Xer, tasks, edges)));
    }

    static Fin<Seq<ScheduleTask>> Tasks(Map<string, string> blocks) => Read(blocks, "TASK", TaskOf);

    static Fin<Seq<TaskRelation>> Edges(Map<string, string> blocks, HashSet<string> known) =>
        Read(blocks, "TASKPRED", EdgeOf).Bind(rows =>
            rows.Find(e => !known.Contains(e.Predecessor) || !known.Contains(e.Successor)).Match(
                Some: e => FinFail<Seq<TaskRelation>>(new ScheduleFault.DanglingEdge($"{e.Predecessor}->{e.Successor}").ToError()),
                None: () => FinSucc(rows)));

    // One SepReader per block: the header indices resolve once outside the row loop, Enumerate lifts
    // each ref-struct Row through the Fin-returning projection, then Traverse threads the faults.
    static Fin<Seq<T>> Read<T>(Map<string, string> blocks, string table, Func<SepReader.Row, FrozenDictionary<string, int>, Fin<T>> project) =>
        blocks.Find(table).Match(
            Some: block => {
                using var sep = Options.FromText(block);
                var ix = sep.Header.ColNames.Select(static (c, i) => (c, i)).ToFrozenDictionary(static p => p.c, static p => p.i, StringComparer.Ordinal);
                return toSeq(sep.Enumerate(row => project(row, ix))).Traverse(identity).As();
            },
            None: () => FinSucc(Seq<T>()));

    static Fin<ScheduleTask> TaskOf(SepReader.Row row, FrozenDictionary<string, int> ix) =>
        from id in Cell(row, ix, "task_code")
        let plannedStart = Date(row, ix, "target_start_date").IfNone(LocalDate.MinIsoValue)
        let plannedFinish = Date(row, ix, "target_end_date").IfNone(plannedStart)
        let duration = Duration.FromHours(Num(row, ix, "target_drtn_hr_cnt"))
        select new ScheduleTask(
            id, Opt(row, ix, "guid"), Or(row, ix, "wbs_id", ""), Or(row, ix, "task_name", id),
            plannedStart, plannedFinish, Date(row, ix, "act_start_date"), Date(row, ix, "act_end_date"),
            duration, Num(row, ix, "phys_complete_pct"),
            Or(row, ix, "rsrc_id", ""), Num(row, ix, "target_qty"), Num(row, ix, "target_cost"),
            Constraint(row, ix), Opt(row, ix, "clndr_id"), Resident(row, ix),
            // Content key over the scheduling-relevant fields (Row.Span is volatile after column
            // access, so the edition derives from the admitted values, never the raw span).
            XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(
                $"{id}|{plannedStart:R}|{plannedFinish:R}|{duration.BclCompatibleTicks}")));

    static Fin<TaskRelation> EdgeOf(SepReader.Row row, FrozenDictionary<string, int> ix) =>
        from pred in Cell(row, ix, "pred_task_id")
        from succ in Cell(row, ix, "task_id")
        select new TaskRelation(pred, succ, RelationKind.OfP6(Or(row, ix, "pred_type", "PR_FS")), Duration.FromHours(Num(row, ix, "lag_hr_cnt")));

    // The %F header line and the %R data lines of one table join into a Sep block; %T opens, %E seals.
    static Map<string, string> Split(TextReader reader) {
        var (blocks, name, lines) = (Map<string, string>(), "", Seq<string>());
        for (var line = reader.ReadLine(); line is not null; line = reader.ReadLine()) {
            var tab = line.IndexOf('\t');
            var marker = tab < 0 ? line : line[..tab];
            var body = tab < 0 ? "" : line[(tab + 1)..];
            (blocks, name, lines) = marker switch {
                "%T" => (blocks, body, Seq<string>()),
                "%F" or "%R" => (blocks, name, lines.Add(body)),
                "%E" => (blocks.AddOrUpdate(name, string.Join('\n', lines)), name, lines),
                _ => (blocks, name, lines),
            };
        }
        return blocks;
    }

    // Header-resolved column accessors over a ref-struct Row; absence and blanks project to Option.
    static Option<string> Get(SepReader.Row row, FrozenDictionary<string, int> ix, string col) =>
        ix.TryGetValue(col, out var i) && row[i].Span is { Length: > 0 } span ? Some(span.ToString()) : None;
    static Fin<string> Cell(SepReader.Row row, FrozenDictionary<string, int> ix, string col) =>
        Get(row, ix, col).ToFin(new ScheduleFault.MalformedTable($"missing-column:{col}").ToError());
    static string Or(SepReader.Row row, FrozenDictionary<string, int> ix, string col, string fallback) => Get(row, ix, col).IfNone(fallback);
    static Option<string> Opt(SepReader.Row row, FrozenDictionary<string, int> ix, string col) => Get(row, ix, col);
    static double Num(SepReader.Row row, FrozenDictionary<string, int> ix, string col) =>
        ix.TryGetValue(col, out var i) ? row[i].TryParse<double>() ?? 0d : 0d;
    static Option<LocalDate> Date(SepReader.Row row, FrozenDictionary<string, int> ix, string col) =>
        Get(row, ix, col).Bind(static s => XerDate.Parse(s) is { Success: true } r ? Some(r.Value.Date) : None);

    // P6's resident solved overlay off the TASK row: present only when the export carries the
    // computed early/late columns (a scheduled XER), absent for an unscheduled import. The float
    // legs TRANSCRIBE P6's stored total_float_hr_cnt/free_float_hr_cnt straight (never re-derived from
    // the date span) so the Drift gate compares the durable store against an INDEPENDENT derivation;
    // negative total_float_hr_cnt survives as the constraint-infeasibility signal.
    static Option<ScheduleFloat> Resident(SepReader.Row row, FrozenDictionary<string, int> ix) =>
        from es in Date(row, ix, "early_start_date")
        from ls in Date(row, ix, "late_start_date")
        select ScheduleFloat.Resident(es, Date(row, ix, "early_end_date").IfNone(es), ls, Date(row, ix, "late_end_date").IfNone(ls),
            Num(row, ix, "total_float_hr_cnt"), Num(row, ix, "free_float_hr_cnt"));

    static Option<TaskConstraint> Constraint(SepReader.Row row, FrozenDictionary<string, int> ix) =>
        from kind in Opt(row, ix, "cstr_type").Bind(ScheduleConstraint.OfP6)
        from on in Date(row, ix, "cstr_date")
        select new TaskConstraint(kind, on);
}

public static class XerDate {
    // P6 XER timestamps: `yyyy-MM-dd HH:mm` (the date leg is the schedule date the network reads).
    public static readonly LocalDateTimePattern Pattern = LocalDateTimePattern.CreateWithInvariantCulture("yyyy-MM-dd HH:mm");
    public static ParseResult<LocalDateTime> Parse(string raw) => Pattern.Parse(raw);
}

// MSPDI document admission through System.Xml.Linq — STJ reads JSON, never XML.
public static class Mspdi {
    static readonly XNamespace N = "http://schemas.microsoft.com/project";

    public static Fin<ScheduleNetwork> Project(XDocument doc) {
        var tasks = toSeq(doc.Descendants(N + "Task").Where(static t => t.Element(N + "UID") is not null));
        return tasks.Traverse(TaskOf).As().Map(rows =>
            new ScheduleNetwork(ScheduleFormat.MsProjectXml, rows, tasks.Bind(EdgesOf)));
    }

    static Fin<ScheduleTask> TaskOf(XElement task) =>
        from id in Opt(task, "UID").ToFin(new ScheduleFault.MalformedTable("mspdi-task-no-uid").ToError())
        let start = Date(task, "Start").IfNone(LocalDate.MinIsoValue)
        select new ScheduleTask(
            id, Opt(task, "GUID"), Opt(task, "OutlineNumber").IfNone(""), Opt(task, "Name").IfNone(id),
            start, Date(task, "Finish").IfNone(start), Date(task, "ActualStart"), Date(task, "ActualFinish"),
            Span(task, "Duration"), Num(task, "PercentComplete"), "", 0d, 0d,
            Constraint(task), None, None, XxHash128.HashToUInt128(Encoding.UTF8.GetBytes(task.ToString())));

    static Seq<TaskRelation> EdgesOf(XElement task) =>
        toSeq(task.Elements(N + "PredecessorLink")).Map(link => new TaskRelation(
            (string?)link.Element(N + "PredecessorUID") ?? "", (string?)task.Element(N + "UID") ?? "",
            RelationKind.OfMsProject((int?)link.Element(N + "Type") ?? 1), Span(link, "LinkLag")));

    static Option<TaskConstraint> Constraint(XElement task) =>
        from kind in Optional((int?)task.Element(N + "ConstraintType")).Bind(ScheduleConstraint.OfMsProject)
        from on in Date(task, "ConstraintDate")
        select new TaskConstraint(kind, on);

    static Option<string> Opt(XElement e, string name) => Optional((string?)e.Element(N + name)).Filter(static s => s.Length > 0);
    static double Num(XElement e, string name) => (double?)e.Element(N + name) ?? 0d;
    static Option<LocalDate> Date(XElement e, string name) =>
        Optional((DateTime?)e.Element(N + name)).Map(static d => LocalDate.FromDateTime(d));

    // MSPDI durations are xsd:duration text (`PT8H0M0S`); XmlConvert parses the ISO span.
    static Duration Span(XElement e, string name) =>
        Opt(e, name).Map(static s => Duration.FromTimeSpan(System.Xml.XmlConvert.ToTimeSpan(s))).IfNone(Duration.Zero);
}

// The constraint-aware forward/backward CPM fold: adjacency derived ONCE, the four pass arms read
// the RelationKind float anchors and clamp to the ScheduleConstraint, the receipt reconciles the
// derived float against P6's resident overlay.
public static class CpmPass {
    public static SchedulePass Resolve(ScheduleNetwork network, LocalDate dataDate) {
        var byId = network.ById;
        var preds = network.Predecessors;
        var succs = network.Successors;
        return Topological(byId.Keys.ToSeq(), network.Relations, succs).Match(
            Fail: _ => new SchedulePass(Map<string, ScheduleFloat>(), dataDate, dataDate, 0, 0, 0, 0, 0),
            Succ: order => {
                // A started/complete activity is pinned to its progress floor (progressed logic reads the
                // actuals against the data date); an unstarted activity floors at its planned start versus
                // the data date and a successor raises that floor by the modality-anchored predecessor date
                // plus lag, then the constraint clamps.
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
                // The drift gate compares at WHOLE-DAY granularity: P6 stores float as work-hours while the
                // re-derivation walks continuous calendar days, so an hours-vs-days `!=` would report
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

    // The resource leveling profile: each scheduled activity spreads its ResourceUnits across its
    // early-start..early-finish day span, the per-resource per-day demand sums, and the peak day per
    // resource projects the over-allocation surface — one fold over the float map and the task set, the
    // resource-loading columns made real rather than asserted.
    public static Seq<ResourceLoad> ResourceProfile(ScheduleNetwork network, SchedulePass pass) =>
        toSeq(network.ById.Values.Filter(static t => t.ResourceId.Length > 0).GroupBy(static t => t.ResourceId)).Map(group => {
            var demand = group.Fold(Map<LocalDate, double>(), (acc, t) => pass.Floats.Find(t.TaskId).Match(
                Some: f => toSeq(Range(0, int.Max(1, Period.DaysBetween(f.EarlyStart, f.EarlyFinish) + 1)))
                    .Fold(acc, (day, n) => day.AddOrUpdate(f.EarlyStart.PlusDays(n), u => u + t.ResourceUnits, t.ResourceUnits)),
                None: () => acc));
            var peak = demand.Fold((Units: 0d, Day: LocalDate.MinIsoValue), static (m, kv) => kv.Value > m.Units ? (kv.Value, kv.Key) : m);
            return new ResourceLoad(group.Key, peak.Units, peak.Day, group.Sum(static t => t.BudgetedCost));
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

| [INDEX] | [FORMAT]       | [SURFACE]                              | [TABLES / ELEMENTS]                                                              |
| :-----: | :------------- | :------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | P6 XER         | `XerTables.Sift` `%T`/`%F`/`%R` reader | `TASK` activities + resident float overlay, `TASKPRED` relations, `PROJWBS` WBS, `CALENDAR` |
|  [02]   | MS-Project XML | `Mspdi.Project` `XDocument` read       | `Task` elements, `PredecessorLink` relations (`System.Xml.Linq`, never STJ)      |

| [INDEX] | [ALGEBRA]            | [SURFACE]                                                     | [LAW]                                                                            |
| :-----: | :------------------- | :------------------------------------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | resident overlay     | `ScheduleFloat.Resident` transcribes `total_float_hr_cnt`/`free_float_hr_cnt` off the `TASK` columns | the durable store TRANSCRIBES P6's stored float (never re-derived from the date span) as the federation overlay |
|  [02]   | critical set         | `SchedulePass.CriticalSet(floatDays)` = total-float-≤-threshold set | the float-zero floor at `0`, the near-critical band above, never the single longest task |
|  [03]   | float pass           | `CpmPass.Resolve` forward/backward over derived adjacency     | the drive is `RelationKind.DriveEarly`/`DriveLate` row behavior over the `(FromFinish, ToFinish)` anchors with SIGNED lag (a lead survives); adjacency derived once |
|  [04]   | constraint clamp     | `ScheduleConstraint.ClampEarly`/`ClampLate` `(Start, Sense)` row behavior | `cstr_type`/`cstr_date` clamp the early/late date; a HARD `On`/`Mandatory` pins both passes; an unclamped pass is the deleted form |
|  [05]   | negative float       | `SchedulePass.NegativeFloat` = total-float-<-zero count       | a hard start-pin earlier than the network drive forces late below early; never hidden by a non-negative clamp |
|  [06]   | reconciliation       | `SchedulePass.Drift` = transcribed ≠ derived total-float count | the re-derivation gates the resident overlay, never replaces it; holds only because the resident leg is transcribed |
|  [07]   | schedule health      | `SchedulePass.Cpli(baselineFinish)` DCMA index               | the critical-path-length index gates schedule realism, a projection on the receipt |
|  [08]   | baseline erosion     | `ScheduleBaseline.Diff` → `Seq<FloatErosion>`                 | multi-baseline variance is a content-key float-erosion diff, never a table       |
|  [09]   | retained/progressed  | `ScheduleTask.ProgressFloor(dataDate)` pins the forward pass  | completed pins actuals, in-progress pins start + `PercentComplete`-remaining; load-bearing actuals, no new field |
|  [10]   | resource leveling    | `CpmPass.ResourceProfile` → `Seq<ResourceLoad>` peak demand  | leveling and over-allocation are a real activity-network fold, never a prose claim |

## [03]-[TS_PROJECTION]

- Owner: `ScheduleFormatKind`, `RelationKindWire`, `ScheduleConstraintWire`, `TaskRelationWire`, `TaskConstraintWire`, `ScheduleTaskWire`, `ScheduleFloatWire`, `FloatErosionWire`, `ResourceLoadWire`, `SchedulePassWire`, `ScheduleBaselineWire` — the durable external-scheduler wire surface the TS-web Gantt, the CPM float overlay, and the resource-histogram decode; the 4D construction-state wire shapes (task-element link, per-element status, as-of state map) are the `Rasm.Bim/schedule` owner's projection the scrubber decodes from the Bim wire, never re-declared here.
- Packages: BCL inbox.
- Growth: a new wire payload is one interface decoded through the same options, zero new surface; a new task field, constraint, or float metric is one optional member on the existing interface.
- Boundary: dates cross as ISO-8601 date strings (NodaTime `LocalDate` round-trip); durations cross as the roundtrip-pattern string; the relation kind crosses as the two-letter code and reconstructs as the literal union; the activity constraint crosses as the P6 `cstr_type` code and the constraint date; the resident P6 float overlay rides `ScheduleTaskWire.resident` so the Gantt distinguishes a scheduler-solved activity from an unscheduled one; the WBS path crosses as the ltree path string so the Gantt renders the hierarchy; the float, pass, resource-load, and baseline-erosion shapes cross primitive-mapped so the CPM overlay renders the threshold-critical set, the negative-float (`ScheduleFloatWire.negative` / `SchedulePassWire.negativeFloat`) infeasibility band, the per-resource over-allocation histogram (`ResourceLoadWire.peakUnits`/`peakDay`), and the slip set; the content-keyed baseline edition crosses as a 32-char lowercase-hex `UInt128` string (`edition.ToString("x32")`), never a `Uint8Array`, so the multi-baseline diff keys by string; the 4D construction-state wire (the task-element link, the per-element status, and the as-of state map) is the Bim schedule owner's projection — the scrubber decodes it from the Bim wire and federates the durable external float overlay to it by the IFC `GlobalId` the `ScheduleTaskWire.globalId` carries (the durable `TaskId` is the P6/MS-Project activity code, never the federation key), so this surface carries only the durable external-scheduler shapes and never a 4D-state record.

```ts contract
type ScheduleFormatKind = "p6-xer" | "ms-project-xml";

type RelationKindWire = "FS" | "SS" | "FF" | "SF";

type ScheduleConstraintWire =
  | "CS_MSO" | "CS_MSOA" | "CS_MSOB" | "CS_MEO" | "CS_MEOB"
  | "CS_MANDSTART" | "CS_MANDFIN" | "CS_ALAP";

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

- [XER_TABLE_GRAMMAR]: the P6 XER `%T`/`%F`/`%R`/`%E` table-marker grammar and the `TASK`/`TASKPRED`/`PROJWBS`/`CALENDAR` column names the per-section `SepReader` projection reads by name — the `task_code`/`wbs_id`/`task_name` identity, the `target_start_date`/`target_end_date`/`act_start_date`/`act_end_date` `yyyy-MM-dd HH:mm` dates, `target_drtn_hr_cnt` duration-in-hours, `phys_complete_pct`, and the `TASKPRED` `pred_task_id`/`task_id`/`pred_type`/`lag_hr_cnt` columns — verified against a real XER export; the RESIDENT computed overlay columns (`early_start_date`/`early_end_date`/`late_start_date`/`late_end_date`/`total_float_hr_cnt`/`free_float_hr_cnt`) are read straight off the `TASK` table and TRANSCRIBED into `ScheduleFloat.Resident` (the float legs read `total_float_hr_cnt`/`free_float_hr_cnt` directly, never re-derived from the date span — a re-derivation would collapse the `Drift` reconciliation into a tautology) so the durable store RESIDES the scheduler-solved float the Bim `CriticalPath` overlays by `GlobalId`, `None` when the export is unscheduled; the `cstr_type`/`cstr_date` primary-constraint columns admit the `ScheduleConstraint` the CPM pass clamps to (`CS_MSO`/`CS_MSOA`/`CS_MSOB`/`CS_MEO`/`CS_MEOB`/`CS_MANDSTART`/`CS_MANDFIN`/`CS_ALAP`), and `clndr_id` carries the calendar correlation; the IFC `GlobalId` federation correlation reads the `TASK` `guid` column (or the `UDFVALUE`/`UDFTYPE` IFC-GUID UDF where the activity carries one), `Option<string>.None` when the export carries no IFC counterpart, so the durable external float overlay federates to the host-neutral CPM/4D domain by `GlobalId` and never by the P6 activity `TaskId`.
- [MSPROJECT_XML_SHAPE]: the MS-Project MSPDI XML `Project`/`Tasks`/`Task`/`PredecessorLink` element shape under the `http://schemas.microsoft.com/project` namespace the `Mspdi.Project` `System.Xml.Linq.XDocument` reader transcribes — STJ reads JSON and cannot parse XML, so the document lane is `XDocument`, the corrected prior illusion; the `Task` `UID`/`Name`/`Start`/`Finish`/`ActualStart`/`ActualFinish`/`Duration`/`PercentComplete` members, the `PredecessorLink` `PredecessorUID`/`Type` (0=FF 1=FS 2=SF 3=SS)/`LinkLag` members, and the `ConstraintType` (0=ASAP 1=ALAP 2=MSO 3=MFO 4=SNET 5=SNLT 6=FNET 7=FNLT)/`ConstraintDate` constraint members verified against a real `.xml` export, and the Compute P6/XER parse companion's canonical-activity-bytes hand-off boundary; the IFC `GlobalId` federation correlation reads the `Task` `GUID`/`ExtendedAttribute` IFC-GUID field (`Option<string>.None` when absent) so the wire join keys the durable overlay to the `Rasm.Bim/schedule` `ConstructionTask.GlobalId`; the 4D construction-state fold over the IFC-projected `SequenceRel` network is the `Rasm.Bim/schedule` owner's concern joined at the wire by that `GlobalId`.
- [RESIDENT_OVERLAY_VS_DERIVATION]: the durable external-scheduler store TRANSCRIBES P6's solved `total_float_hr_cnt`/`free_float_hr_cnt` as the federation overlay (`ScheduleFloat.Resident`, never re-derived from the date columns) AND re-derives an independent `ScheduleFloat` through `CpmPass.Resolve` over the `TaskRelation` DAG (`ScheduleFloat.Window`) — the forward pass derives early start as the maximum over predecessors of the `RelationKind`-anchored predecessor date (`FromFinish` reads the predecessor finish, else its start) plus the SIGNED edge lag (a negative `lag_hr_cnt` is a lead the drive preserves at whole-day resolution), the backward pass the symmetric late finish, both CLAMPED to the `ScheduleConstraint` `(Start, Sense)` row behavior (`NoLater` caps, `NoEarlier` floors, `On`/`Mandatory` hard-pin both passes, `ALAP` defers) and the data date — a hard start-pin (`On`/`Mandatory`) earlier than the predecessor drive forces the late date below the early date, the source of negative total float `Period.DaysBetween(EarlyStart, LateStart)` the `NegativeFloat` count surfaces, free float the per-modality `RelationKind.FreeGap` successor gap less the edge lag — and the `SchedulePass.Drift` count reconciles the derived total float against the transcribed overlay so a re-sequenced plan whose stored float is stale surfaces as drift rather than silently trusting either source, a reconciliation that is only non-trivial because the resident leg reads the stored hours rather than re-deriving the same span the derivation walks; the adjacency is derived once into `Predecessors`/`Successors` maps and `Topological` runs Kahn over it, a residual cycle aborting `ScheduleFault.CyclicNetwork`, so the prior `byId.Values.Filter(...)` O(V·E) rescan per node is eliminated; the calendar-blind day arithmetic is the deliberate split from the Bim `WorkCalendar.Advance` working-time fold — the durable store reads P6's already-calendar-resolved dates and re-derives in continuous days as the drift gate, while the host-neutral Bim owner owns the IFC work-calendar CPM.
