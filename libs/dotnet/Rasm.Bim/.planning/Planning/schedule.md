# [BIM_SCHEDULE]

The host-neutral 4D construction-sequencing and CPM DOMAIN owner: one `ScheduleNetwork` record carrying the activity network — the `ConstructionTask` row whose `IfcTaskTime` schedule and actual start/finish fold onto NodaTime `Interval`s over the work-calendar zone, whose authored `IfcTaskTime.Completion` percent feeds the cost earned-value join, whose `TaskKind` (`IfcTask.PredefinedType` — the construction/demolition/installation 4D modality) discriminates the playback read, and whose `ProjectStage` places it on the cross-national lifecycle axis the `VividOrange.Stages` taxonomy governs (the `[SmartEnum<string>]` keyed `(Governance.Name, Id)` with its `StageCategory` normalization column, so a RIBA, HOAI, CSLP, and AB89 phase reconcile as rows rather than as an interface ladder at a call site, and the same taxonomy's compiled `IGovernance.Country` pin yields the project's national `ICountry` through `StageLabels.Nation` — the typed context the eurocode annex bridge, the stage-gated report, and the COBie handover register each read instead of a free country string), the `SequenceRel` edge record carrying each `IfcRelSequence` dependency lag as a NodaTime `Period`, discriminated by the `SequenceKind` `[SmartEnum]` (`FinishToStart`/`StartToStart`/`FinishToFinish`/`StartToFinish` as a 2x2 of `FromFinish`/`ToFinish` behaviour columns, never four identical-payload union arms), the `TaskAssignment` projecting `IfcRelAssignsToProcess` to join a task to the assigned `Node.Object` set, and the `ScheduleProjection.Project` fold that folds the GeometryGym `IfcWorkPlan`→`IfcWorkSchedule`→`IfcTask` container into the typed network — the plan-to-schedule and summary-to-detail `IfcRelNests` WBS tree flattened so a nested work-breakdown orders alongside its top-level activities — plus the `WorkCalendar` working-time fold over the public `IfcWorkCalendar.ExceptionTimes` spans (the `WorkingTimes` recurrence is schema-internal in GeometryGym, so the work-week/shift ride the calendar record), the `CriticalPath` forward/backward-pass fold over the `SequenceRel` DAG topologically ordered through the shared `QuikGraph` graph-algorithm owner (`SourceFirstBidirectionalTopologicalSort` over a transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` — each dependency riding its edge as the `Tag` so the pass reads predecessors and successors off `InEdges`/`OutEdges`, the same managed graph-algorithm owner the `Model/systems#SYSTEM_TRACE` reachability and `Review/versioning#VERSION_GRAPH` common-ancestor walks share, never three bespoke graph walks), and the `ConstructionState.At(Instant)` snapshot that selects the shared element set whose task `Interval.Contains` the queried instant. The schedule is a VIEW of the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph`, never a re-modeled element graph and never the retired `BimModel`: a task carries its assigned-element IFC GlobalIds, the snapshot resolves them against the one `ElementGraph` through the `Model/query#ELEMENT_SET` `ByAttribute(ObjectAttribute.GlobalId, …)` predicate matching each `Node.Object.ExternalId`, and the `Rasm.AppUi/Charts` schedule report and the `Rasm.Persistence/Query/federation#PLAN_INGRESS` activity-network persistence consume the typed network by reference. The schedule is HOST-NEUTRAL — the calendar values ride NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalTime`/`IsoDayOfWeek` over the model's work-calendar zone and never a BCL `DateTime` on a public signature — and the task-to-element join is the `Model/query#ELEMENT_SET` `BimTerm` algebra selecting the assigned `Node.Object` set, never a second selection surface. The activity network is the `Planning/cost#ESTIMATE` 5D pairing's time axis: a `ConstructionResource` joins `Planning/schedule#SCHEDULE` `ConstructionTask` by its `GlobalId` (the 5D resource-to-activity pairing) and the task `Completion` is the schedule-performance signal the `Planning/cost#EARNED_VALUE` `EarnedValue` fold reads off the `ConstructionTask`, so this page authors the `[2]-[SCHEDULE]` `ConstructionTask` anchor the cost resource-join cluster cites by reference rather than re-deriving the activity network. The `ScheduleCpm.Schedule` forward/backward pass is the SINGLE CPM owner the whole workspace shares: its `SequenceRel` edge set originates EITHER from this owner's `IfcWorkPlan` projection OR from a `Rasm.Persistence/Ingest/schedule#DURABLE_NETWORK` MPXJ-parsed durable network (the P6 XER / MS-Project round-trip store — it maps each `TaskRelation`'s `DependencyKind`/lag onto a `SequenceRel` edge and correlates its float overlay to this owner's `ConstructionTask.GlobalId`, never the external P6 `TaskId`), so Persistence supplies edges plus the durable store while the forward/backward float pass is THIS owner's one fold — a Persistence `CpmPass` re-deriving the order, or a Bim `CriticalPath` reading a raw P6 relation, is the cross-package drift defect the relocation forbids. A schedule rejection lowers onto `Model/faults#FAULT_BAND` `BimFault` (band 2600, `Fault`-derived), the typed case lifting BARE onto the `Fin<T>` result with no `.ToError()` hop: a task assigning, or a dependency naming, a GlobalId the network never declares is `BimFault.Refused` with `BimReason.DanglingReference` (the contract-element `task-assigns-absent-element`, the dependency `schedule-dependency-absent-process`), a cyclic dependency the forward pass cannot topologically order or a task finishing before it starts is the same `BimFault.Refused` case with `BimReason.Rejected`.

## [01]-[INDEX]

- [02]-[SCHEDULE]: `ScheduleNetwork` record, the `ConstructionTask` record (`IfcTaskTime` as a NodaTime `Interval` + the authored `Completion` + its `ProjectStage`), the `SequenceRel` edge record carrying a `Period` lag discriminated by the `SequenceKind` `[SmartEnum]` (the FS/SS/FF/SF 2x2 modality with `FromFinish`/`ToFinish` behaviour columns), the `TaskAssignment` record, the `TaskStatus`/`TaskKind`/`WorkScheduleKind` `[SmartEnum]` vocabularies, the `ProjectStage` lifecycle discriminant keyed `(Governance.Name, Id)` over the VividOrange stage taxonomy with its `StageCategory` cross-national column and the `StageLabels` caller-composed free-label admission in front of it (`StageLabels.Nation` the project's typed national context), the `ScheduleProjection.Project` fold from the GeometryGym `IfcWorkPlan`→`IfcWorkSchedule`→`IfcTask` surface (the `IfcRelNests` WBS tree flattened), and the `ConstructionState.At(Instant, ConstructionPhase)` phase-partitioned contract-element snapshot.
- [03]-[CRITICAL_PATH]: the `WorkCalendar` working-time fold over the public `IfcWorkCalendar.ExceptionTimes` spans (`DateInterval` exception spans, `AnnualDate` recurring-holiday rows, the `WorkingBetween` working-content measure feeding the pass — the `WorkingTimes` recurrence is schema-internal, per `[04]-[WORK_CALENDAR_RECURRENCE]`), the `CpmStage` lane roster the pass opens its stage boundaries on, the `CriticalPath` value record per task, and the `ScheduleCpm.Schedule` forward/backward-pass CPM fold extending `ScheduleNetwork` over the `SequenceRel`-tagged DAG by the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` Kahn orders (source-first forward, sink-first backward) — the SINGLE CPM owner both the IFC and the MPXJ lanes feed.

## [02]-[SCHEDULE]

- Owner: `ScheduleNetwork` the single host-neutral 4D activity-network record carrying the task set, the dependency-edge set, the assignment set, the work-calendar zone the calendar values resolve against, and the `(GeometryKey, ScheduleKey)` content-key identity the AppUi report and Persistence federation read it by; `ConstructionTask` the task row promoting one `IfcTask`/`IfcTaskTime` into a first-class owner carrying its stable `GlobalId`, name, `TaskStatus`, its `TaskKind` (the `IfcTask.PredefinedType` 4D modality — a `DEMOLITION` task's elements leave the model at its finish where a `CONSTRUCTION` task's arrive, the AppUi playback discriminant), its `TaskGrain` deciding whether the row is a zero-content event or a work-content span, the scheduled `Interval` and the optional actual `Interval` (both over the model's work-calendar zone), the optional authored `Completion` percent, the `WorkScheduleKind` the controlling `IfcWorkSchedule` predefines, the optional `CalendarGlobalId` naming the `IfcWorkCalendar` the task itself assigns, and the optional `TaskDuration` carrying the authored `IfcTaskTime.ScheduleDuration` under the `DurationBasis` row its `DurationType` selects; `TaskGrain` the two-row [SmartEnum] carrying the work-content ELECTION as delegate data (an activity electing between its authored duration and its window content, a milestone answering zero whatever the source stamped); `DurationBasis` the `[SmartEnum<string>]` over `IfcTaskDurationEnum` whose rows carry the working-content projection as delegate data (`WORKTIME` counting shifts of the task's own calendar, `ELAPSEDTIME` measuring the content inside its own calendar span, `NOTDEFINED` folding onto `WORKTIME`); `SequenceRel` the ONE dependency-edge record carrying its predecessor/successor task GlobalIds and the dependency `Period` lag the `IfcRelSequence.TimeLag` `IfcLagTime` declares, discriminated by `SequenceKind` the `[SmartEnum<string>]` over the four IFC modalities — `FinishToStart` (the predecessor's finish gates the successor's start), `StartToStart` (the two starts gated together), `FinishToFinish` (the two finishes gated together), `StartToFinish` (the predecessor's start gates the successor's finish, the just-in-time edge) — collapsed to a closed 2x2 of two behaviour columns (`FromFinish` anchoring the predecessor finish vs start, `ToFinish` anchoring the successor finish vs start) the CPM shift reads, never four identical-payload union arms; `TaskAssignment` the `IfcRelAssignsToProcess` row joining one task to the assigned `Node.Object` GlobalId set; `TaskStatus` the `[SmartEnum<string>]` over the IFC task status; `TaskKind` the `[SmartEnum<string>]` over `IfcTaskTypeEnum`; `WorkScheduleKind` the `[SmartEnum<string>]` over `IfcWorkScheduleTypeEnum`; `ProjectStage` the `[SmartEnum<string>]` lifecycle discriminant keyed `(Governance.Name, Id)` over the `VividOrange.Stages` taxonomy, its rows the Whitby-Wood international baseline with each key DERIVED from its own roster class, carrying the `StageCategory` column; `StageCategory` the `[SmartEnum<string>]` cross-national category axis whose rows hold the VividOrange category-interface membership law as delegate data, so a RIBA, HOAI, CSLP, and AB89 phase reconcile on one axis; `StageLabels` the caller-composed `(Roster, Fallback)` free-label admission resolving the verbatim `IfcProject.Phase` text onto a row rung-major ahead of `ProjectStage.Of`, and carrying the project's national context as the derived `Nation` read over its own roster's governing bodies; `ConstructionState` the snapshot owner folding the network at one `Instant` into a `Model/query#ELEMENT_SET` `ElementQuery`; `ScheduleProjection` the static fold over the GeometryGym `IfcWorkPlan` container. `ScheduleNetwork` carries the `Map<string, WorkCalendar> Calendars` its tasks assign plus the `DefaultCalendar` the work plan declares, both folded once and rebased onto the network zone at projection, and `CalendarFor` is the per-task election every consumer reads.
- Cases: `SequenceKind` rows `FinishToStart` · `StartToStart` · `FinishToFinish` · `StartToFinish` (4), each a `(FromFinish, ToFinish)` cell of the 2x2 modality space, and `SequenceRel` the one `(PredecessorGlobalId, SuccessorGlobalId, Period Lag, SequenceKind Kind)` edge record — the `IfcSequenceEnum.START_FINISH` member resolves to the first-class `StartToFinish` row rather than collapsing onto `FinishToStart`, and the `USERDEFINED`/`NOTDEFINED` fallbacks fold onto `FinishToStart` through the `SequenceKind.Of` resolver so an unknown modality defaults to the dominant edge without minting a parallel edge type; the `ConstructionTask` row carries its `GlobalId`, `Name`, `TaskStatus`, `TaskKind`, `TaskGrain`, the `Interval Scheduled` schedule window, the `Option<Interval> Actual` actual window (present once the task carries an `ActualStart`/`ActualFinish`), the `Option<double> PercentComplete` (the `IfcTaskTime.Completion` ratio, `None` when unset so the earned-value fold falls back to the actual-interval fraction), the `WorkScheduleKind`, and the `Option<ProjectStage> Stage` the plan's own lifecycle phase stamps (`None` on an unstaged model, never a fabricated construction phase) — a milestone is a zero-duration `Interval` whose start equals its finish, while a spanning activity carries a positive-duration `Interval`; the `TaskAssignment` row carries its `TaskGlobalId` and the `Seq<string>` of assigned-element GlobalIds the `IfcRelAssignsToProcess.RelatedObjects` `IfcProduct` set names.
- Entry: `ScheduleProjection.Project(IfcWorkPlan plan, ElementGraph graph, DateTimeZone zone, Option<ProjectStage> stage)` folds one GeometryGym work-plan container into one `ScheduleNetwork` — materializing the plan's nested `IfcWorkSchedule` set once through the `IfcRelNests` decomposition, folding each schedule's controlled `IfcTask` set AND each task's nested sub-task tree onto flat `ConstructionTask` rows (reading the `TaskTime` `IfcTaskTime` schedule/actual start-finish onto the `Interval`s over `zone`, the `Completion` onto the percent, and the `Status`/`PredefinedType`/`IsMilestone` onto the typed `TaskStatus`/`TaskKind`/`TaskGrain` rows), folding each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges (resolving `SequenceType` onto the `SequenceKind` modality through `SequenceRel.Of` and reading the `TimeLag` `IfcLagTime` onto the `Period`), folding each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows binding the assigned-`IfcProduct` GlobalIds, and deriving the `(GeometryKey, ScheduleKey)` identity — and `ScheduleProjection.ProjectAll(Seq<IfcWorkPlan> plans, ElementGraph graph, DateTimeZone zone, Option<ProjectStage> stage)` lifts every work plan in a model onto the `Seq<ScheduleNetwork>` the report reads; `ProjectStage.Of(IStage stage)` is the ONE lifecycle admission — an exact `(Governance.Name, Id)` hit, else the international category the stage's own VividOrange interface names — so a RIBA-4, an HOAI-LP5, and a CSLP-EXE phase land the same `DetailedDesign` row and a schedule filters, groups, and orders on that axis without an `is IConstruction` ladder anywhere, while `StageLabels.Resolve(string)` is its free-label front — the caller-composed roster-and-fallback admission the model's verbatim `IfcProject.Phase` text (the `Projection/semantic` `ProjectAttributeSet` round-trip lane) resolves through before `Of` ever sees an `IStage` — and `StageLabels.Nation` is the same composition's typed national context, the distinct `ICountry` its roster's own national governing bodies pin (`None` where the roster is the bare international baseline or spans two national scales), the value `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime.Of` elects the Eurocode regime from and a stage-gated report and the `Exchange/export#COBIE_EMIT` handover register each scope by; `Fin<T>` aborts on a task assigning a product GlobalId the element graph never declares (`Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.DanglingReference`) or a task whose schedule finish precedes its start (`BimFault.Refused` with `BimReason.Rejected`), the typed case lifting bare. `ConstructionState.At(ScheduleNetwork network, ElementGraph graph, Instant instant, Option<ConstructionPhase> phase = default)` reads the element set whose tasks hold the requested `ConstructionPhase` at the instant — `Active` the default in-flight read over the effective `Interval` (the actual window when present, else the scheduled window), `Completed` the finished-by read a progress QTO and an installed-by-milestone selection key, `Pending` the not-yet-started complement, each phase's membership law a delegate row so the fold carries no phase switch, unioning each active task's `TaskAssignment` element GlobalIds, and resolving them against the element graph through the `Model/query#ELEMENT_SET` `ByAttribute(ValueMatch.Exact(GlobalId), ValueMatch.OneOf(…))` term into one `ElementQuery`, so the 4D playback at instant `t` is one fold over the network into one query, never an enumerated per-task arm and never a second store.
- Auto: `Project` reads the `IfcWorkPlan` runtime graph and folds it into the typed network — `SchedulesOf` materializes the plan's nested `IfcWorkSchedule` set once through `plan.IsNestedBy` `IfcRelNests.RelatedObjects` (the GeometryGym work-control decomposition path, distinct from `Controls` which reaches tasks), `ControlledTasks` materializes each schedule's controlled `IfcTask` set through `schedule.Controls` `IfcRelAssignsToControl.RelatedObjects` and flattens each task's `IsNestedBy` `IfcRelNests` sub-task tree so a summary→detail WBS folds onto the flat row set the CPM orders, and `TasksOf` threads each row's `WorkScheduleKind` from the owning schedule's `PredefinedType` and dedups on `GlobalId` so a task reached through both control and nesting orders once; the `TaskOf` projection reads `IfcTask.TaskTime` onto the scheduled `Interval` through `IntervalOf(taskTime.ScheduleStart, taskTime.ScheduleFinish, zone)`, the optional actual `Interval` through `ActualOf(taskTime.ActualStart, taskTime.ActualFinish, zone)`, and the `IfcTaskTime.Completion` onto the optional percent (each BCL `DateTime` lifting to a NodaTime `LocalDateTime` via `LocalDateTime.FromDateTime`, mapping into `zone` leniently through `InZoneLeniently` to absorb the daylight-transition gap/overlap an IFC calendar value carries no offset for, and projecting to the `Instant` the `Interval` bounds); `SequencesOf` folds each task's `IsPredecessorTo` `IfcRelSequence` set onto `SequenceRel` edges discriminating `SequenceType` through `SequenceRel.Of` and reading the `TimeLag` `IfcLagTime.LagValue` `IfcDuration` onto the `Period` through `PeriodOf` (building the NodaTime `Period` from the decomposed `Years`/`Months`/`Days`/`Hours`/`Minutes`/`Seconds` integer fields, never a fragile `ToString()` round-trip); `AssignmentsOf` folds each task's `OperatesOn` `IfcRelAssignsToProcess` set onto `TaskAssignment` rows reading the `RelatedObjects` `IfcProduct` GlobalIds; the `ScheduleNetwork.BindAssignments` fold resolves each assignment's element GlobalIds against the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph.ObjectNodes` `ExternalId` index so a task assigning a product the graph never declares aborts the projection, and the `ScheduleNetwork.Identity` fold derives the `(GeometryKey, ScheduleKey)` `UInt128` pair the AppUi report and Persistence federation read the network by, both minted through the kernel seed-zero `ContentHash.Of` over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` fold (length-prefixed strings under per-run `Ordinal` counts, `I64` interval bounds) — `GeometryKey` over the ordinally-sorted assigned-element GlobalIds so the report re-reads only a network whose assigned geometry changed, and `ScheduleKey` over the `GlobalId`-ordered task effective-`Interval`/status/authored-percent/`TaskKind`/`WorkScheduleKind`/`ProjectStage`/`TaskGrain` rows and the `(predecessor, successor, kind, lag)`-ordered dependency edges so a re-sequenced (a re-kinded SS→FF edge included), re-statused, re-progressed, re-staged, or re-kinded (a CONSTRUCTION task re-authored DEMOLITION — the 4D playback discriminant) plan re-renders — the canonical sort making both keys invariant to the unstable `IfcSet` iteration order a re-parse yields. `ConstructionState.At` folds the network at the instant: the `active` fold filters the task set to those whose effective `Interval` contains the instant through `Interval.Contains`, the `assigned` fold unions the active tasks' `TaskAssignment` element GlobalIds into one distinct GlobalId set, and the `Model/query#ELEMENT_SET` `ElementQuery.Query` over the `ByAttribute(ObjectAttribute.GlobalId, ValueMatch.OneOf(assigned))` term resolves the active-element set against the element graph — so a Gantt scrub at instant `t` materializes the in-progress element set as a `Model/query#ELEMENT_SET` value the AppUi viewport renders.
- Output: the `Seq<ScheduleNetwork>` is the 4D activity-network evidence the `Rasm.AppUi/Charts` schedule report reads by the `(GeometryKey, ScheduleKey)` reference and the `Rasm.Persistence/Query/federation#PLAN_INGRESS` activity-network persistence stores by GlobalId, the `ConstructionState.At(Instant)` snapshot is the `Model/query#ELEMENT_SET` `ElementQuery` the AppUi 4D viewport renders at a Gantt instant, and the `ConstructionTask` row is the `Planning/cost#ESTIMATE` 5D cost pairing's time axis the `ConstructionResource` joins by `GlobalId` and the `Planning/cost#EARNED_VALUE` fold reads the `Completion` percent off; a scheduled milestone task, a start-to-finish dependency lag, and an assigned-element set each carry their typed calendar value on one network record, the `TaskKind` row is the construct-vs-demolish modality the 4D playback discriminates per task, and the `ProjectStage` row is the lifecycle axis a stage-gated report, a `Semantics/properties#PROPERTY_TEMPLATES` handover-scope audit, and a COBie register each read off one vocabulary — the `StageLabels.Nation` beside it the national context those same readers scope by and the `Model/eurocode#EUROCODE_ALGEBRA` `AnnexRegime` elects its design regime from, so the lifecycle taxonomy answers WHEN and WHERE from one composed declaration.
- Packages: GeometryGymIFC_Core, NodaTime, VividOrange.Countries, VividOrange.Stages, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm.Element, Rasm
- Growth: a new dependency modality is one `SequenceKind` row (its `FromFinish`/`ToFinish` cell) the column-driven CPM shift reads with zero new arm, reached from the next `IfcSequenceEnum` member; a new task status is one `TaskStatus` row; a new task kind is one `TaskKind` row reading the next `IfcTaskTypeEnum` member; a new task grain (a hammock, a level-of-effort span) is one `TaskGrain` row carrying its own work-content delegate, the CPM reading it with no new arm; a new work-schedule kind is one `WorkScheduleKind` row reading the next `IfcWorkScheduleTypeEnum` member; a new international lifecycle phase is one `StageCategory` row carrying its membership delegate plus one `ProjectStage` row over the baseline class that realizes it, and a new national scale needs neither — its phases normalize through the category axis the taxonomy already declares and its nation reaches `StageLabels.Nation` as one roster entry, never a country column authored anywhere; a per-task stage override is one `TaskOf` read the same `Option<ProjectStage>` column absorbs; a new task time field (the `IfcTaskTime` `RemainingTime`, `StatusTime`) is one column on `ConstructionTask` read by the same fold; a new work plan rides the existing `ProjectAll` fold on one row; never a per-task-status record, never a second schedule store, never a `Get<Status>` task family, and never a re-modeled element graph on the schedule.
- Boundary: the task grain is the `TaskGrain` ROW and a stored `bool IsMilestone` column is the deleted form — the flag had exactly two readers, the projection that set it and the content key that hashed it, so it discriminated nothing while a task carrying both the flag and a stray `ScheduleDuration` advanced its finish across working days its own zero-length window denied; the dependency edge is ONE `SequenceRel` record discriminated by the `SequenceKind` `[SmartEnum]` over the four modalities — a `FinishToStartRel`/`StartToStartRel`/`FinishToFinishRel`/`StartToFinishRel` record family, four identical-payload union arms with triplicated `PredecessorGlobalId`/`SuccessorGlobalId`/`Lag` accessor switches, or four sibling factory methods is the deleted form mirroring the no-per-element-class law at `Model/elements#IFC_CLASS`, the modality variation carrying NO payload variation so it is a discriminant value not a case shape; the lifecycle axis is the `ProjectStage` ROW vocabulary keyed `(Governance.Name, Id)` and every cross-national reconciliation runs through the `StageCategory` interface-backed rows — an `is IConstruction` ladder at a call site, a `stage.Id` parse or `Name` compare outside the one `StageLabels.Resolve` rung ladder, and a hand-rolled RIBA/HOAI phase enum beside the taxonomy are each the deleted form, and a `ProjectStage` key spelled as a literal rather than derived from its own baseline class is the stale-spelling defect the `Key` derivation forecloses — the row takes the baseline class itself and derives every column from it; the national context is the typed `ICountry` the taxonomy's own `IGovernance.Country` pin yields through `ProjectStage.Nation`, read once at the caller-composed `StageLabels.Nation` roster, and a free country string on the network or a task, a second nation enum, and a nation read off the Whitby-Wood `International` body (whose `Country` is that body's own `UnitedKingdom` domicile, so reading it keys every international project onto the British annex) are each the deleted form, as is a `ProjectStage` nation COLUMN, which reads `None` on all ten baseline rows and so declares a capability no consumer can act on; the `ConstructionTask` carries its calendar value as a NodaTime `Interval` over the model's work-calendar `DateTimeZone` and a BCL `DateTime`/`DateTimeOffset` field on the task or a public projection signature is the named host-neutrality defect — the IFC `DateTime` crosses the projection boundary once at `IntervalOf` and never reaches a domain signature, the schedule consuming the full NodaTime `Interval`/`Period`/`ZonedDateTime`/`LocalDateTime`/`Interval.Contains` surface for the calendar arithmetic; the GeometryGym `IfcWorkPlan`/`IfcWorkSchedule`/`IfcTask`/`IfcTaskTime`/`IfcRelSequence`/`IfcLagTime`/`IfcRelAssignsToProcess`/`IfcRelNests` surface is consumed as settled vocabulary through the `IfcProcess`/`IfcTask` discrimination and a hand-rolled task reader is the deleted form; the plan→schedule link is the `IfcRelNests` decomposition (`IsNestedBy`) and reading schedules off `Controls` (the task-control path) is the named projection defect, while the WBS sub-task tree is the `IfcTask.IsNestedBy` recursion and a flat top-level-only task read is the deleted form; the task-to-element join is the shared `Rasm.Element/Graph/element#ELEMENT_GRAPH` `ElementGraph` resolved through the `Model/query#ELEMENT_SET` `ByAttribute(ObjectAttribute.GlobalId, …)` predicate matching each `Node.Object.ExternalId`, and the retired `BimModel`/`BimElement` element record or a public-constructor selection over a second store is the deleted form — the schedule produces the assigned GlobalIds, the query owns the resolution, and a parallel schedule-element selection arm is the no-second-selection-surface reject; the `(GeometryKey, ScheduleKey)` content-key identity is derived through the ONE kernel seed-zero `ContentHash` over the kernel `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` fold across ordinally-sorted id/row sets (invariant to the unstable `IfcSet` iteration order), the hasher law the `Review/diff#MODEL_DIFF` owner already rules — a hand-rolled `XxHash128`/`Encoding.UTF8` string-join preimage, whose delimiter and section-marker choices can forge an equality between two decompositions of one concatenation and whose `:R` ratio render a culture or runtime revision can move, and minting a second identity scheme for the report join are the named defects — and the AppUi report and Persistence federation read the network by that reference; the `Planning/cost#ESTIMATE` 5D resource-join cluster cites this `[2]-[SCHEDULE]` `ConstructionTask` anchor by reference and re-deriving the activity network on the cost page is the deleted form; a schedule rejection raises `BimFault.Refused` with its closed scope and reason and lifts the typed `BimFault` case BARE onto the `Fin<T>` result, so a `.ToError()` lowering hop or a literal case construction bypassing the kernel admission context is the named contract defect.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using GeometryGym.Ifc;
using LanguageExt;
using Rasm.Bim.Model;
using NodaTime;
using NodaTime.Text;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Query;
using Thinktecture;
using VividOrange.Countries;
using VividOrange.Stages;
using Baseline = VividOrange.Stages;
using ContentHash = Rasm.Domain.ContentHash;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Planning;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class TaskStatus {
    public static readonly TaskStatus NotYetStarted = new("NOTYETSTARTED");
    public static readonly TaskStatus Started       = new("STARTED");
    public static readonly TaskStatus Completed     = new("COMPLETED");
    public static readonly TaskStatus Delayed       = new("DELAYED");
    public static readonly TaskStatus NotDefined    = new("NOTDEFINED");

    public static TaskStatus Of(string? status) =>
        TryGet(status?.Trim(), out TaskStatus? row) ? row : NotDefined;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class WorkScheduleKind {
    public static readonly WorkScheduleKind Actual      = new("ACTUAL");
    public static readonly WorkScheduleKind Baseline    = new("BASELINE");
    public static readonly WorkScheduleKind Planned     = new("PLANNED");
    public static readonly WorkScheduleKind UserDefined = new("USERDEFINED");
    public static readonly WorkScheduleKind NotDefined  = new("NOTDEFINED");

    public static WorkScheduleKind Of(IfcWorkScheduleTypeEnum kind) =>
        TryGet(kind.ToString(), out WorkScheduleKind? row) ? row : NotDefined;
}

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

    public static TaskKind Of(IfcTaskTypeEnum kind) =>
        TryGet(kind.ToString(), out TaskKind? row) ? row : NotDefined;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class StageCategory {
    public static readonly StageCategory Brief            = new("brief",             static s => s is IBrief);
    public static readonly StageCategory Competition      = new("competition",       static s => s is ICompetition);
    public static readonly StageCategory Idea             = new("idea",              static s => s is IIdea or IPredesign);
    public static readonly StageCategory ConceptualDesign = new("conceptual-design", static s => s is IConceptualDesign);
    public static readonly StageCategory SchematicDesign  = new("schematic-design",  static s => s is ISchematicDesign);
    public static readonly StageCategory DetailedDesign   = new("detailed-design",   static s => s is IDetailedDesign);
    public static readonly StageCategory Construction     = new("construction",      static s => s is IConstruction);
    public static readonly StageCategory Handover         = new("handover",          static s => s is IHandover);
    public static readonly StageCategory InUse            = new("in-use",            static s => s is IInUse);
    public static readonly StageCategory EndOfLife        = new("end-of-life",       static s => s is IEndOfLife);

    [UseDelegateFromConstructor]
    public partial bool Holds(IStage stage);

    public static Option<StageCategory> Of(IStage stage) => toSeq(Items).Find(row => row.Holds(stage));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ProjectStage {
    public static readonly ProjectStage Idea            = new(new Baseline.Idea(),            StageCategory.Idea);
    public static readonly ProjectStage Brief           = new(new Baseline.Brief(),           StageCategory.Brief);
    public static readonly ProjectStage Competition     = new(new Baseline.Competition(),     StageCategory.Competition);
    public static readonly ProjectStage ConceptDesign   = new(new Baseline.ConceptDesign(),   StageCategory.ConceptualDesign);
    public static readonly ProjectStage SchematicDesign = new(new Baseline.SchematicDesign(), StageCategory.SchematicDesign);
    public static readonly ProjectStage DetailedDesign  = new(new Baseline.DetailedDesign(),  StageCategory.DetailedDesign);
    public static readonly ProjectStage Construction    = new(new Baseline.Construction(),    StageCategory.Construction);
    public static readonly ProjectStage Handover        = new(new Baseline.Handover(),        StageCategory.Handover);
    public static readonly ProjectStage InUse           = new(new Baseline.InUse(),           StageCategory.InUse);
    public static readonly ProjectStage EndOfLife       = new(new Baseline.EndOfLife(),       StageCategory.EndOfLife);

    public StageCategory Category { get; }

    private ProjectStage(IStage stage, StageCategory category) : this(Key(stage)) => Category = category;

    public static string Key(IStage stage) => $"{stage.Governance.Name}:{stage.Id}";

    public static Option<ICountry> Nation(IStage stage) =>
        stage.Governance is Baseline.International ? None : Some(stage.Governance.Country);

    public static Option<ProjectStage> Of(IStage stage) =>
        TryGet(Key(stage), out ProjectStage? row) && row is { } exact
            ? Some(exact)
            : StageCategory.Of(stage).Bind(category => toSeq(Items).Find(candidate => candidate.Category == category));
}

public sealed record StageLabels(Seq<IStage> Roster, Option<ProjectStage> Fallback) {
    public static readonly StageLabels International = new(
        toSeq<IStage>([
            new Baseline.Idea(), new Baseline.Brief(), new Baseline.Competition(), new Baseline.ConceptDesign(),
            new Baseline.SchematicDesign(), new Baseline.DetailedDesign(), new Baseline.Construction(),
            new Baseline.Handover(), new Baseline.InUse(), new Baseline.EndOfLife()]),
        None);

    public Option<ICountry> Nation => Nations is { Count: 1 } sole ? sole.Head : None;

    Seq<ICountry> Nations => toSeq(Roster
        .Choose(ProjectStage.Nation)
        .DistinctBy(static country => country.CountryCode, StringComparer.Ordinal));

    public Option<ProjectStage> Resolve(string phaseLabel) =>
        Optional(phaseLabel.Trim()).Filter(static label => label.Length > 0).Bind(label =>
            Rung(label, static (l, s) => string.Equals(l, ProjectStage.Key(s), StringComparison.OrdinalIgnoreCase))
          | Rung(label, static (l, s) => string.Equals(l, s.Id, StringComparison.OrdinalIgnoreCase))
          | Rung(label, static (l, s) => string.Equals(l, s.Name, StringComparison.OrdinalIgnoreCase)))
        | Fallback;

    Option<ProjectStage> Rung(string label, Func<string, IStage, bool> match) =>
        Roster.Find(stage => match(label, stage)).Bind(ProjectStage.Of);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class SequenceKind {
    public static readonly SequenceKind FinishToStart  = new("FINISH_START",  fromFinish: true,  toFinish: false);
    public static readonly SequenceKind StartToStart   = new("START_START",   fromFinish: false, toFinish: false);
    public static readonly SequenceKind FinishToFinish = new("FINISH_FINISH", fromFinish: true,  toFinish: true);
    public static readonly SequenceKind StartToFinish  = new("START_FINISH",  fromFinish: false, toFinish: true);

    public bool FromFinish { get; }
    public bool ToFinish { get; }

    public static SequenceKind Of(IfcSequenceEnum kind) => kind switch {
        IfcSequenceEnum.START_START   => StartToStart,
        IfcSequenceEnum.FINISH_FINISH => FinishToFinish,
        IfcSequenceEnum.START_FINISH  => StartToFinish,
        _                             => FinishToStart,
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class DurationBasis {
    public static readonly DurationBasis WorkTime = new("WORKTIME", static (calendar, from, span) => {
        LocalDateTime anchor = from.InZone(calendar.Zone).LocalDateTime;
        LocalDateTime shifted = anchor + span;
        int days = Period.Between(anchor, shifted, PeriodUnits.Days).Days;
        return calendar.ShiftLength * days
             + (shifted.InZoneLeniently(calendar.Zone).ToInstant()
              - anchor.PlusDays(days).InZoneLeniently(calendar.Zone).ToInstant());
    });

    public static readonly DurationBasis ElapsedTime = new("ELAPSEDTIME", static (calendar, from, span) =>
        calendar.WorkingBetween(from, (from.InZone(calendar.Zone).LocalDateTime + span).InZoneLeniently(calendar.Zone).ToInstant()));

    [UseDelegateFromConstructor]
    public partial Duration Content(WorkCalendar calendar, Instant from, Period span);

    public static DurationBasis Of(IfcTaskDurationEnum kind) =>
        kind == IfcTaskDurationEnum.ELAPSEDTIME ? ElapsedTime : WorkTime;
}

[SmartEnum<string>]
public sealed partial class TaskGrain {
    public static readonly TaskGrain Activity = new("activity", static (calendar, authored, scheduled) =>
        authored.Match(
            Some: duration => duration.Content(calendar, scheduled.Start),
            None: () => calendar.WorkingBetween(scheduled.Start, scheduled.End)));

    public static readonly TaskGrain Milestone = new("milestone", static (_, _, _) => Duration.Zero);

    [UseDelegateFromConstructor]
    public partial Duration Content(WorkCalendar calendar, Option<TaskDuration> authored, Interval scheduled);

    public static TaskGrain Of(bool milestone, Interval scheduled) =>
        milestone || scheduled.Duration <= Duration.Zero ? Milestone : Activity;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TaskDuration(Period Span, DurationBasis Basis) {
    public Duration Content(WorkCalendar calendar, Instant from) => Basis.Content(calendar, from, Span);
}

public sealed record SequenceRel(string PredecessorGlobalId, string SuccessorGlobalId, Period Lag, SequenceKind Kind) {
    public static SequenceRel Of(IfcSequenceEnum kind, string predecessor, string successor, Period lag) =>
        new(predecessor, successor, lag, SequenceKind.Of(kind));
}

public sealed record ConstructionTask(
    string GlobalId,
    string Name,
    TaskStatus Status,
    TaskKind Kind,
    TaskGrain Grain,
    WorkScheduleKind ScheduleKind,
    Option<ProjectStage> Stage,
    Option<string> CalendarGlobalId,
    Option<TaskDuration> Authored,
    Interval Scheduled,
    Option<Interval> Actual,
    Option<double> PercentComplete) {
    public Interval Effective => Actual.IfNone(Scheduled);

    public bool ActiveAt(Instant instant) => Effective.Contains(instant);

    public Duration WorkContent(WorkCalendar calendar) => Grain.Content(calendar, Authored, Scheduled);
}

public sealed record TaskAssignment(string TaskGlobalId, Seq<string> ElementGlobalIds);

public sealed record ScheduleNetwork(
    string GlobalId,
    string Name,
    DateTimeZone Zone,
    Seq<ConstructionTask> Tasks,
    Seq<SequenceRel> Dependencies,
    Seq<TaskAssignment> Assignments,
    Map<string, WorkCalendar> Calendars,
    WorkCalendar DefaultCalendar) {
    public WorkCalendar CalendarFor(ConstructionTask task) =>
        task.CalendarGlobalId.Bind(Calendars.Find).IfNone(DefaultCalendar);

    public (UInt128 GeometryKey, UInt128 ScheduleKey) Identity => (
        GeometryKeyOf(toSeq(Assignments.Bind(static a => a.ElementGlobalIds).OrderBy(static id => id, StringComparer.Ordinal))),
        ScheduleKeyOf(
            toSeq(Tasks.OrderBy(static t => t.GlobalId, StringComparer.Ordinal)),
            toSeq(Dependencies
                .OrderBy(static d => d.PredecessorGlobalId, StringComparer.Ordinal)
                .ThenBy(static d => d.SuccessorGlobalId, StringComparer.Ordinal)
                .ThenBy(static d => d.Kind.Key, StringComparer.Ordinal)
                .ThenBy(static d => PeriodPattern.Roundtrip.Format(d.Lag), StringComparer.Ordinal))));

    static UInt128 GeometryKeyOf(Seq<string> assigned) =>
        ContentHash.Of(assigned, static (ids, writer) =>
            ids.Fold(writer.Ordinal(ids.Count), static (w, id) => w.String(id)));

    static UInt128 ScheduleKeyOf(Seq<ConstructionTask> tasks, Seq<SequenceRel> edges) =>
        ContentAddress.Of((tasks, edges), 0.0, static (s, writer) => s.edges
            .Fold(s.tasks
                .Fold(writer.Ordinal(s.tasks.Count), static (w, task) => w
                    .String(task.GlobalId)
                    .I64(task.Effective.Start.ToUnixTimeTicks())
                    .I64(task.Effective.End.ToUnixTimeTicks())
                    .String(task.Status.Key)
                    .Double(task.PercentComplete.IfNone(0d))
                    .String(task.Kind.Key)
                    .String(task.ScheduleKind.Key)
                    .String(task.Stage.Map(static row => row.Key).IfNone(""))
                    .String(task.CalendarGlobalId.IfNone(""))
                    .String(task.Authored.Map(static a => $"{a.Basis.Key}:{PeriodPattern.Roundtrip.Format(a.Span)}").IfNone(""))
                    .String(task.Grain.Key))
                .Ordinal(s.edges.Count),
                static (w, edge) => w
                    .String(edge.PredecessorGlobalId)
                    .String(edge.SuccessorGlobalId)
                    .String(edge.Kind.Key)
                    .String(PeriodPattern.Roundtrip.Format(edge.Lag)))).Value;

    public Fin<ScheduleNetwork> BindAssignments(ElementGraph graph) {
        var index = toHashSet(graph.ObjectNodes.Choose(static o => o.ExternalId));
        return Assignments
            .Bind(static a => a.ElementGlobalIds)
            .Find(id => !index.Contains(id))
            .Match(
                Some: id => Fin.Fail<ScheduleNetwork>(new BimFault.Refused(BimScope.Planning, BimReason.DanglingReference, string.Join(':', new object?[] { "task-assigns-absent-element", id }))),
                None: () => Fin.Succ(this));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ScheduleProjection {
    static Option<string> TextOf(string? value) =>
        Optional(value).Map(static text => text.Trim()).Filter(static text => text.Length > 0);

    public static Fin<ScheduleNetwork> Project(IfcWorkPlan plan, ElementGraph graph, DateTimeZone zone, Option<ProjectStage> stage) {
        var tasks = TasksOf(plan).Filter(static entry => entry.Task.TaskTime is { } time && time.ScheduleStart != default);
        return tasks
            .TraverseM(entry => TaskOf(entry.Task, entry.Kind, stage, zone))
            .As()
            .Map(rows => new ScheduleNetwork(
                plan.GlobalId,
                TextOf(plan.Name).IfNone(""),
                zone,
                rows,
                SequencesOf(tasks.Map(static entry => entry.Task)),
                AssignmentsOf(tasks.Map(static entry => entry.Task)),
                CalendarsOf(tasks.Map(static entry => entry.Task), zone),
                DefaultCalendarOf(plan, zone)))
            .Bind(network => network.BindAssignments(graph));
    }

    public static Fin<Seq<ScheduleNetwork>> ProjectAll(Seq<IfcWorkPlan> plans, ElementGraph graph, DateTimeZone zone, Option<ProjectStage> stage) =>
        plans.TraverseM(plan => Project(plan, graph, zone, stage)).As();

    static Seq<IfcWorkSchedule> SchedulesOf(IfcWorkPlan plan) =>
        toSeq(plan.IsNestedBy
            .SelectMany(static rel => rel.RelatedObjects.OfType<IfcWorkSchedule>()));

    static Seq<(IfcTask Task, WorkScheduleKind Kind)> TasksOf(IfcWorkPlan plan) =>
        toSeq(SchedulesOf(plan)
            .Bind(static schedule => ControlledTasks(schedule)
                .Map(task => (Task: task, Kind: WorkScheduleKind.Of(schedule.PredefinedType))))
            .DistinctBy(static entry => entry.Task.GlobalId));

    static Seq<IfcTask> ControlledTasks(IfcWorkSchedule schedule) =>
        toSeq(schedule.Controls
            .SelectMany(static rel => rel.RelatedObjects.OfType<IfcTask>())
            .SelectMany(NestedTasks));

    static Seq<IfcTask> NestedTasks(IfcTask task) =>
        Seq(task) + toSeq(task.IsNestedBy
            .SelectMany(static rel => rel.RelatedObjects.OfType<IfcTask>()))
            .Bind(NestedTasks);

    static Fin<ConstructionTask> TaskOf(IfcTask task, WorkScheduleKind kind, Option<ProjectStage> stage, DateTimeZone zone) =>
        IntervalOf(task.TaskTime?.ScheduleStart, task.TaskTime?.ScheduleFinish, zone)
            .Map(scheduled => new ConstructionTask(
                task.GlobalId,
                TextOf(task.Name).IfNone(""),
                TaskStatus.Of(task.Status),
                TaskKind.Of(task.PredefinedType),
                TaskGrain.Of(task.IsMilestone, scheduled),
                kind,
                stage,
                CalendarIdOf(task),
                DurationOf(task.TaskTime),
                scheduled,
                ActualOf(task.TaskTime?.ActualStart, task.TaskTime?.ActualFinish, zone),
                CompletionOf(task.TaskTime)));

    static Option<string> CalendarIdOf(IfcTask task) =>
        CalendarsAssigned(task).Head.Map(static calendar => calendar.GlobalId);

    static Seq<IfcWorkCalendar> CalendarsAssigned(IfcObjectDefinition definition) =>
        toSeq(definition.HasAssignments
            .OfType<IfcRelAssignsToControl>()
            .Select(static rel => rel.RelatingControl)
            .OfType<IfcWorkCalendar>());

    static Map<string, WorkCalendar> CalendarsOf(Seq<IfcTask> tasks, DateTimeZone zone) =>
        tasks.Bind(CalendarsAssigned).Fold(Map<string, WorkCalendar>(), (map, calendar) =>
            map.ContainsKey(calendar.GlobalId) ? map : map.Add(calendar.GlobalId, WorkCalendar.Of(calendar, zone)));

    static WorkCalendar DefaultCalendarOf(IfcWorkPlan plan, DateTimeZone zone) =>
        CalendarsAssigned(plan).Head.Match(
            Some: calendar => WorkCalendar.Of(calendar, zone),
            None: () => WorkCalendar.Default.In(zone));

    static Option<TaskDuration> DurationOf(IfcTaskTime? taskTime) =>
        taskTime?.ScheduleDuration is { } duration
            ? Some(new TaskDuration(PeriodOf(duration), DurationBasis.Of(taskTime.DurationType)))
                .Filter(static authored => !authored.Span.Equals(Period.Zero))
            : None;

    static Seq<SequenceRel> SequencesOf(Seq<IfcTask> tasks) =>
        toSeq(tasks.SelectMany(static task => task.IsPredecessorTo.OfType<IfcRelSequence>()))
            .Choose(static rel =>
                from predecessor in TextOf(rel.RelatingProcess?.GlobalId)
                from successor in TextOf(rel.RelatedProcess?.GlobalId)
                select SequenceRel.Of(rel.SequenceType, predecessor, successor, PeriodOf(rel.TimeLag)));

    static Seq<TaskAssignment> AssignmentsOf(Seq<IfcTask> tasks) =>
        tasks
            .Map(static task => new TaskAssignment(
                task.GlobalId,
                toSeq(task.OperatesOn
                    .SelectMany(static rel => rel.RelatedObjects.OfType<IfcProduct>())
                    .Select(static product => product.GlobalId)
                    .Where(static id => id.Length > 0))))
            .Filter(static assignment => assignment.ElementGlobalIds.IsEmpty == false);

    static Fin<Interval> IntervalOf(DateTime? start, DateTime? finish, DateTimeZone zone) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => Fin.Succ(new Interval(from, to)),
            ({ } from, { } to)                 => Fin.Fail<Interval>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "task-finish-before-start", InstantPattern.ExtendedIso.Format(to), InstantPattern.ExtendedIso.Format(from) }))),
            ({ } from, null)                   => Fin.Succ(new Interval(from, from)),
            _                                  => Fin.Fail<Interval>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "task-missing-schedule-start" }))),
        };

    static Option<Interval> ActualOf(DateTime? start, DateTime? finish, DateTimeZone zone) =>
        (InstantOf(start, zone), InstantOf(finish, zone)) switch {
            ({ } from, { } to) when to >= from => Some(new Interval(from, to)),
            ({ } from, _)                      => Some(new Interval(from, from)),
            _                                  => None,
        };

    static Instant? InstantOf(DateTime? value, DateTimeZone zone) =>
        value is { } moment && moment != default
            ? LocalDateTime.FromDateTime(moment).InZoneLeniently(zone).ToInstant()
            : null;

    static Option<double> CompletionOf(IfcTaskTime? taskTime) =>
        taskTime is { Completion: var completion } && completion is > 0d and <= 1d ? Some(completion) : None;

    static Period PeriodOf(IfcLagTime? lag) => PeriodOf(lag?.LagValue as IfcDuration);

    static Period PeriodOf(IfcDuration? duration) =>
        duration is { } span
            ? new PeriodBuilder {
                Years = span.Years, Months = span.Months, Days = span.Days,
                Hours = span.Hours, Minutes = span.Minutes,
                Seconds = (long)Math.Round(span.Seconds),
            }.Build()
            : Period.Zero;
}

[SmartEnum<string>]
public sealed partial class ConstructionPhase {
    public static readonly ConstructionPhase Completed = new("completed", static (effective, t) => effective.End <= t);
    public static readonly ConstructionPhase Active    = new("active",    static (effective, t) => effective.Contains(t));
    public static readonly ConstructionPhase Pending   = new("pending",   static (effective, t) => effective.Start > t);

    [UseDelegateFromConstructor]
    public partial bool Holds(Interval effective, Instant instant);
}

public static class ConstructionState {
    public static ElementQuery At(ScheduleNetwork network, ElementGraph graph, Instant instant, Option<ConstructionPhase> phase = default) {
        ConstructionPhase holds = phase.IfNone(ConstructionPhase.Active);
        var matched = toHashSet(network.Tasks.Filter(task => holds.Holds(task.Effective, instant)).Map(static task => task.GlobalId));
        var assigned = toHashSet(network.Assignments
            .Filter(assignment => matched.Contains(assignment.TaskGlobalId))
            .Bind(static assignment => assignment.ElementGlobalIds)).ToSeq();
        return ElementQuery.Query(graph,
            BimLeaf.Of(new ElementLeaf.ByAttribute(
                new ValueMatch.Exact(new PropertyValue.Text(ObjectAttribute.GlobalId.Key)),
                new ValueMatch.OneOf(assigned))));
    }
}
```

## [03]-[CRITICAL_PATH]

- Owner: `WorkCalendar` the host-neutral working-time function folded from one `IfcWorkCalendar` — a work-week `IsoDayOfWeek` set, a daily shift `LocalTime` span, the inclusive `DateInterval` exception spans the holiday/weather `IfcWorkTime` windows project onto (one typed span per window, never a hand-expanded concrete-date set), and the `AnnualDate` recurring-holiday rows an external calendar feed lands (the P6/MS-Project recurring exception a flat date set structurally cannot carry) — that maps a working-content `Duration` onto a calendar finish past the non-working days, reads the working content of an arbitrary span, and enumerates that span's non-working gaps; `CpmStage` the pass's OWN closed stage roster projecting the `Model/observability#HOOKS` `StageMark` onto the `rasm.bim.planning.progress` observe point; `CriticalPath` the per-task value record carrying `EarlyStart`/`EarlyFinish`/`LateStart`/`LateFinish` as `Instant`, `TotalFloat`/`FreeFloat` as `Duration`, and the `bool IsCritical` zero-total-float flag; `ScheduleCpm.Schedule` the forward/backward-pass CPM fold EXTENDING `ScheduleNetwork` over the `SequenceRel` adjacency by topological order producing the `Map<string, CriticalPath>` per-task float window — one immutable fold over the edge set the network already owns, the SINGLE CPM owner both the IFC-projected and the MPXJ-parsed `SequenceRel` edge sets feed, never a mutable PERT accumulator and never a second pass in Persistence.
- Law: the calendar is PER TASK — every walk, working-content measure, and float bound resolves on `network.CalendarFor(task)`, the task's own assigned `IfcWorkCalendar` falling back to the network default, so a six-day concrete crew and a five-day commissioning crew never grade each other's float; a single network-wide calendar parameter is the deleted form. The task duration ELECTS between two tiers — the authored `IfcTaskTime.ScheduleDuration` read under its own `DurationType` basis row, else the working content of the scheduled window — because an authored duration states work content while the window states only placement, and reading the window alone misprices every task whose bounds carry float. `BarrenLimit` exhaustion and an empty work-week both REJECT typed onto `BimFault.Refused` with `BimReason.Rejected`: returning the seed instant fabricates a working instant no calendar admits, and every float derived from it reads as a plausible schedule.
- Exemption: `Advance`, `Recede`, and `WorkingBetween` are the page's `[EXPRESSION_SPINE]` measured kernels — bounded day walks that clamp a cursor into each shift window and accumulate remaining content, statement-bodied because the walk consumes a mutable remainder against a day-stepped cursor and no expression fold expresses the early exit inside a shift without materializing a day sequence per call on the CPM's hottest path.
- Entry: `WorkCalendar.Of(IfcWorkCalendar calendar, DateTimeZone zone)` folds one GeometryGym work-calendar container into the typed working-time function — reading each `ExceptionTimes` `IfcWorkTime` window's public `StartDate`/`FinishDate` onto ONE inclusive NodaTime `DateInterval` span, over the default construction work-week and shift because the GeometryGym `IfcWorkTime.RecurrencePattern` (the weekday/shift recurrence) is schema-internal with no public accessor, while the `AnnualHolidays` `AnnualDate` column lands a recurring annual holiday from a calendar feed; `WorkCalendar.Default` is the standard 5-day Monday-through-Friday 08:00–16:00 calendar a plan without an `IfcWorkCalendar` resolves against, so a duration-driven task always has a working-time function, and `WorkCalendar.ShiftLength` is the one shift's content the `WORKTIME` duration basis multiplies. `ScheduleCpm.Schedule(this ScheduleNetwork network, Option<BimHooks> hooks = default)` folds the network into the `Fin<Map<string, CriticalPath>>`, each stage boundary OPENING its declared `CpmStage` fraction on the optional hooks so a ten-thousand-task network publishes monotone positions instead of one silent block, — the `SequenceRel` dependencies fold into a transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` (the task `GlobalId` the vertex, each dependency riding its value edge as the `Tag` so `InEdges`/`OutEdges` ARE the predecessor/successor reads, `allowParallelEdges: true` keeping a real SS+FF pair between one task pair as two constraints) and `graph.SourceFirstBidirectionalTopologicalSort()` IS the `QuikGraph` Kahn order the forward pass folds in (a residual cycle the `IsDirectedAcyclicGraph()` pre-gate rejects, lowered to `BimFault.Refused` with `BimReason.Rejected`, rather than a hand-rolled in-degree drain), the forward pass folds each task in that order deriving `EarlyStart` as the maximum over `InEdges` of the lagged, modality-anchored predecessor float — the `SequenceKind.FromFinish` column anchors the predecessor `EarlyFinish` (`FinishToStart`/`FinishToFinish`) or `EarlyStart` (`StartToStart`/`StartToFinish`), the edge `Period` lag applies as a calendar offset in the model zone (`anchor.InZone(zone).LocalDateTime + lag`, never the throwing `Period.ToDuration`), and a `SequenceKind.ToFinish` finish-anchored modality (`FinishToFinish`/`StartToFinish`) RECEDES the successor working content off the lagged finish through `calendar.Recede` so the early start skips non-working days rather than a raw `Duration` subtraction — and `EarlyFinish` through `calendar.Advance(EarlyStart, workContent)`, the backward pass folds in the sink-first `SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward)` order — the API's own reverse Kahn order, never an `order.Rev()` re-derivation — deriving `LateFinish` as the minimum over `OutEdges` of the `BackShift` bound (the exact backward DUAL of `Shift` over the same `SequenceKind` 2x2: the `ToFinish` column anchors the successor `LateFinish` vs `LateStart`, the lag RECEDES as the same zone-anchored calendar `Period`, and a start-anchored modality ADVANCES the predecessor working content so the bound lands on its finish) and `LateStart` through `calendar.Recede`, and `TotalFloat = LateStart − EarlyStart`, `FreeFloat` the minimum out-edge SLACK (the successor's achieved `EarlyStart` minus this edge's `Shift` demand — exact over all four modalities and lags, collapsing to `min successor EarlyStart − EarlyFinish` only on an unlagged FS graph), `IsCritical = TotalFloat ≤ Duration.Zero`; the `Fin<T>` result carries the cycle rejection, the dangling-process rejection, and the calendar-exhaustion rejection every walk lifts.
- Auto: `WorkCalendar.Advance(Instant from, Duration work)` walks forward from `from` in the model zone, consuming each working day's REMAINING shift content (the cursor clamped up to `ShiftStart`, then run to `ShiftEnd` — a mid-shift start contributes only its tail, a non-working/exception day zero) until the accumulated working `Duration` reaches `work` and the finish lands INSIDE the shift, never past `ShiftEnd`, so a three-working-day task starting on a Friday resolves its finish on the following Tuesday across the intervening weekend; `Recede` is the symmetric backward walk the backward pass reads, the cursor clamped down to `ShiftEnd`; `ShiftWindow(LocalDate)` is the day's `[ShiftStart, ShiftEnd]` working window (skipped on a non-working/exception day) the two walks clamp into, `NonWorking(Instant, Instant)` is its COMPLEMENT — the merged spans of the same walk carrying no working content, the enumerable form a consumer drawing or reasoning over the gaps needs and the measured `WorkingBetween` cannot express — and `WorkingBetween(Instant, Instant)` sums each working day's OVERLAP with that window, the derived tier of the `ConstructionTask.WorkContent` election — so the CPM feeds `Advance` either the authored `TaskDuration` resolved through its `DurationBasis` row or the TRUE working content of the scheduled window, never the raw calendar span the interval bounds, which double-counts the weekends and partial off-shift hours the calendar already skips; `WorkingBetween` folds the inclusive `DateInterval` day sequence directly (the interval IS the `IEnumerable<LocalDate>` walk) and `SpanOf` projects each exception window onto one `DateInterval` value, so no hand-rolled day-range arithmetic exists beside the NodaTime interval owner. `ScheduleCpm.Schedule` threads the `(forward, backward)` accumulator as two `Fin`-result-returning folds over the one topological order, each task's walk running on its OWN elected calendar: the forward fold seeds a no-predecessor task at the project start (the minimum scheduled `Interval.Start`), reads the predecessor `CriticalPath` already computed (topological order guarantees it is present), and the backward fold seeds a no-successor task at the project finish (the maximum forward `EarlyFinish`); a task whose `SequenceKind` modality is `StartToStart`/`FinishToFinish`/`StartToFinish` reads its `FromFinish`/`ToFinish` columns in BOTH passes (`Shift` forward, `BackShift` backward — the two duals over the one 2x2) rather than assuming finish-to-start, so the float algebra is column-driven over all four modalities and a new modality needs no new shift arm. The topological orders are the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` source-first and sink-first Kahn sorts over the transient `BidirectionalGraph` folded once from the `SequenceRel`-tagged edges — the graph is the algorithm input only, never a domain field, its `InEdges`/`OutEdges` the one predecessor/successor surface the float fold reads (the `GroupBy` side maps are the deleted form) — so the schedule, the MEP trace, and the commit-DAG merge-base share ONE graph-algorithm owner rather than three hand-rolled walks, and an MPXJ-parsed `ProjectFile` whose `Relation` edges map onto the same `SequenceRel` set feeds this one pass rather than a second `CpmPass` in Persistence.
- Output: the `Map<string, CriticalPath>` is the CPM evidence the `Rasm.AppUi/Charts` critical-chain report and the 4D playback read — the critical-path set is `path.Filter(static (_, cp) => cp.IsCritical)`, the per-task float window the resource-leveling read, and the `EarlyStart`/`LateFinish` the schedule bounds; the `WorkCalendar` working-time finish is the calendar-accurate `Interval` the `ConstructionState.At` snapshot reads, never a continuous-span approximation, while the `Planning/cost#EARNED_VALUE` `EarnedValue` schedule-performance fold reads the `ConstructionTask.Completion` progress off this network.
- Packages: GeometryGymIFC_Core, NodaTime, QuikGraph, Thinktecture.Runtime.Extensions, LanguageExt.Core, Rasm
- Growth: a new dependency modality is one `SequenceKind` row (its `FromFinish`/`ToFinish` cell) the column-driven CPM shift reads with no new arm, the same row the `[2]-[SCHEDULE]` `[SmartEnum]` widens on; a new recurring holiday is one `AnnualDate` row on the `AnnualHolidays` column and a new exception window one `DateInterval` span, both the same `IsWorking` read every walk and the `NonWorking` complement share (a per-day shift override stays one future column on `WorkCalendar`); a new authored-duration basis is one `DurationBasis` row carrying its working-content delegate, reached from the next `IfcTaskDurationEnum` member with no fold edit; a new governance checkpoint is one `CpmStage` row whose declared fraction the observe point reads with no arithmetic elsewhere; a new float metric (a working-day float, a critical-chain buffer) is one column on `CriticalPath` derived from the same forward/backward pass; a new graph query over the network (a longest-path duration, a resource-constrained reorder) rides the same `QuikGraph` `AlgorithmExtensions` facade over the transient `BidirectionalGraph`; never a per-modality CPM pass, never a second calendar engine, never a hand-rolled topological sort beside `QuikGraph`, and never a mutable PERT accumulator.
- Boundary: the CPM pass is ONE immutable fold over the `SequenceRel` adjacency by topological order — a mutable `Dictionary<string, double>` early-start accumulator mutated in a `for` loop is the deleted form, the forward/backward pass threading the `Map<string, CriticalPath>` accumulator through the topological fold; the topological orders are the `QuikGraph` `SourceFirstBidirectionalTopologicalSort` source-first/sink-first Kahn sorts over the transient `BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>` folded from the tagged dependencies, and a hand-rolled in-degree drain over a `Map<>` adjacency, a `GroupBy` predecessor/successor side map beside the graph's own `InEdges`/`OutEdges`, or an `order.Rev()` re-derivation of the sink-first order the API yields directly, is the deleted form (`QuikGraph` owns the ORDER and the edge-carried `SequenceRel`, the `WorkCalendar` fold owns the float/calendar arithmetic), as is a backward gate reading every out-edge as finish-to-start — dropping the modality columns and the lag misprices the float of every lagged or SS/FF/SF dependency, so the backward pass runs the `BackShift` dual of the same 2x2 — a cyclic dependency edge surfaces through `IsDirectedAcyclicGraph()` lowered BARE to `BimFault.Refused` with `BimReason.Rejected` rather than looping, and a walk that exhausts `BarrenLimit` or meets an empty work-week lifts the same case rather than returning its seed instant — the seed-return is the deleted form, a fabricated working instant no calendar admits that every downstream float then reads as real; the transient graph is the algorithm input only and never a domain field on `ScheduleNetwork`, the `(GeometryKey, ScheduleKey)` content key keying the fold so the order re-runs only on a changed network; the CPM consumes the `ConstructionTask.WorkContent` election as the `Advance`/`Recede` duration and feeding the raw calendar span (which double-counts non-working days the calendar skips) is the named correctness defect, as is dropping the authored `ScheduleDuration` for a window-derived measure; the ZONE authority is the network's — `ScheduleProjection` rebases every folded calendar through `WorkCalendar.In(zone)` at the fold, since `WorkCalendar.Default` carries `Utc` and an unrebased calendar-less plan resolves its shifts, its working content, and its whole float algebra in a zone the task `Interval`s were never built in; that rebase is a MEMBER because a sealed record's copy constructor is private and a caller-side `with` does not compile; `NonWorking` is the ONE enumerable non-working surface, so a consumer rendering or reasoning over calendar gaps reads it rather than re-deriving the work-week, the exception spans, and the annual rows against this calendar — that re-derivation drifts the moment a project declares an exception window and is the deleted form; the `WorkCalendar` working-time arithmetic composes the NodaTime `LocalDate`/`LocalTime`/`DateInterval`/`AnnualDate`/`IsoDayOfWeek` surface — the exception window is one inclusive `DateInterval` value and the recurring holiday one `AnnualDate` row — and a hand-rolled day-counter, a concrete expanded date set, or a BCL `DateTime.AddDays` loop is the deleted form — NodaTime owns the date arithmetic; the GeometryGym `IfcWorkCalendar.WorkingTimes`/`ExceptionTimes` `SET<IfcWorkTime>` and the public `IfcWorkTime.StartDate`/`FinishDate` `DateTime` spans are consumed as settled vocabulary, while the schema-internal `IfcWorkTime.RecurrencePattern`/`IfcRecurrencePattern` carries NO public accessor at the admitted pin — reading `RecurrencePattern.WeekdayComponent` off a different assembly is the named phantom-member defect, the calendar resolving the public exception spans over the default work-week instead; the `CriticalPath` float window is the `Planning/cost#EARNED_VALUE` `EarnedValue` schedule-performance read and re-deriving the activity network on the cost page is the deleted form; the CPM is THIS owner's single fold and a Persistence `CpmPass` re-deriving the order over MPXJ-parsed edges is the named cross-package drift defect; a CPM rejection lifts the typed `BimFault` case BARE, a `.ToError()` hop being the named contract defect.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using GeometryGym.Ifc;
using LanguageExt;
using NodaTime;
using NodaTime.Text;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.TopologicalSort;
using Rasm.Bim.Model;
using Thinktecture;
using BimHooks = Rasm.Domain.HookSet<Rasm.Bim.Model.BimPoint, Rasm.Bim.Model.BimFact, Rasm.Domain.TelemetrySource>;
using static LanguageExt.Prelude;

namespace Rasm.Bim.Planning;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CpmStage {
    public static readonly CpmStage Ordered  = new(done: 0.00, witness: "order");
    public static readonly CpmStage Forward  = new(done: 0.15, witness: "forward");
    public static readonly CpmStage Backward = new(done: 0.55, witness: "backward");
    public static readonly CpmStage Floated  = new(done: 0.85, witness: "float");
    public static readonly CpmStage Settled  = new(done: 1.00, witness: "settle");

    public double Done { get; }
    public string Witness { get; }

    public StageMark Mark => new(Done, Witness);

    public Unit Beat(Option<BimHooks> hooks) =>
        hooks.IfSome(live => ignore(live.Fire(BimPoint.PlanningProgress, new BimFact.Progress(ProgressLane.Planning, Mark))));
}

// --- [MODELS] --------------------------------------------------------------------------
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

    public Duration ShiftLength => Duration.FromNanoseconds(ShiftEnd.NanosecondOfDay - ShiftStart.NanosecondOfDay);

    bool IsWorking(LocalDate day) =>
        WorkWeek.Contains(day.DayOfWeek)
        && !ExceptionSpans.Exists(span => span.Contains(day))
        && !AnnualHolidays.Exists(holiday => holiday.InYear(day.Year) == day);

    (Instant Lo, Instant Hi) ShiftWindow(LocalDate day) =>
        ((day + ShiftStart).InZoneLeniently(Zone).ToInstant(), (day + ShiftEnd).InZoneLeniently(Zone).ToInstant());

    const int BarrenLimit = 3660;

    public Fin<Instant> Advance(Instant from, Duration work) {
        if (work <= Duration.Zero) { return Fin.Succ(from); }
        if (WorkWeek.Count == 0) { return Fin.Fail<Instant>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "work-calendar-empty-work-week" }))); }
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
                    if (remaining <= available) { return Fin.Succ(start + remaining); }
                    remaining -= available;
                }
            } else { barren++; }
            cursor = (day.PlusDays(1) + ShiftStart).InZoneLeniently(Zone).ToInstant();
        }
        return Fin.Fail<Instant>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "work-calendar-barren", "forward", InstantPattern.ExtendedIso.Format(from), DurationPattern.Roundtrip.Format(work) })));
    }

    public Fin<Instant> Recede(Instant to, Duration work) {
        if (work <= Duration.Zero) { return Fin.Succ(to); }
        if (WorkWeek.Count == 0) { return Fin.Fail<Instant>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "work-calendar-empty-work-week" }))); }
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
                    if (remaining <= available) { return Fin.Succ(end - remaining); }
                    remaining -= available;
                }
            } else { barren++; }
            cursor = (day.PlusDays(-1) + ShiftEnd).InZoneLeniently(Zone).ToInstant();
        }
        return Fin.Fail<Instant>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "work-calendar-barren", "backward", InstantPattern.ExtendedIso.Format(to), DurationPattern.Roundtrip.Format(work) })));
    }

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

    public Seq<Interval> NonWorking(Instant from, Instant to) =>
        to <= from
            ? Seq<Interval>()
            : new DateInterval(from.InZone(Zone).Date, to.InZone(Zone).Date).AsIterable()
                .Fold(Seq<Interval>(), (spans, day) => {
                    var (lo, hi) = ShiftWindow(day);
                    var dayStart = day.AtStartOfDayInZone(Zone).ToInstant();
                    var dayEnd = day.PlusDays(1).AtStartOfDayInZone(Zone).ToInstant();
                    return (IsWorking(day)
                        ? Seq((Lo: dayStart, Hi: lo), (Lo: hi, Hi: dayEnd))
                        : Seq((Lo: dayStart, Hi: dayEnd)))
                        .Fold(spans, (carried, piece) => Joined(carried, Clamp(piece, from, to)));
                });

    static Option<Interval> Clamp((Instant Lo, Instant Hi) piece, Instant from, Instant to) {
        var lo = piece.Lo > from ? piece.Lo : from;
        var hi = piece.Hi < to ? piece.Hi : to;
        return hi > lo ? Some(new Interval(lo, hi)) : None;
    }

    static Seq<Interval> Joined(Seq<Interval> spans, Option<Interval> piece) =>
        piece.Match(
            Some: span => spans.Last.Match(
                Some: open => open.End >= span.Start
                    ? spans.Take(spans.Count - 1).Add(new Interval(open.Start, span.End))
                    : spans.Add(span),
                None: () => Seq(span)),
            None: () => spans);

    public static WorkCalendar Of(IfcWorkCalendar calendar, DateTimeZone zone) =>
        Default with { ExceptionSpans = calendar.ExceptionTimes.AsIterable().Choose(SpanOf).ToSeq(), Zone = zone };

    public WorkCalendar In(DateTimeZone zone) => this with { Zone = zone };

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ScheduleCpm {
    public static Fin<Map<string, CriticalPath>> Schedule(this ScheduleNetwork network, Option<BimHooks> hooks = default) =>
        network.Tasks.IsEmpty
            ? Fin.Succ(Map<string, CriticalPath>())
            : Opened(CpmStage.Ordered, hooks).Bind(_ => Graph(network)).Bind(graph => {
                var calendars = network.Tasks.Map(t => (t.GlobalId, network.CalendarFor(t))).ToMap();
                var duration = network.Tasks.Map(t => (t.GlobalId, t.WorkContent(calendars[t.GlobalId]))).ToMap();
                var projectStart = network.Tasks.Min(static t => t.Scheduled.Start);

                return Opened(CpmStage.Forward, hooks)
                    .Bind(_ => Forward(graph, calendars, duration, projectStart))
                    .Bind(forward => {
                        var projectFinish = forward.Values.Map(static p => p.Ef).Max(projectStart);
                        return Opened(CpmStage.Backward, hooks)
                            .Bind(_ => Backward(graph, calendars, duration, projectFinish))
                            .Bind(backward => Opened(CpmStage.Floated, hooks)
                                .Bind(_ => Paths(graph, network, calendars, forward, backward, duration)))
                            .Bind(paths => Opened(CpmStage.Settled, hooks).Map(_ => paths));
                    });
            });

    static Fin<Unit> Opened(CpmStage stage, Option<BimHooks> hooks) => Fin.Succ(stage.Beat(hooks));

    static Fin<Map<string, (Instant Es, Instant Ef)>> Forward(
        BidirectionalGraph<string, STaggedEdge<string, SequenceRel>> graph, Map<string, WorkCalendar> calendars,
        Map<string, Duration> duration, Instant projectStart) =>
        toSeq(graph.SourceFirstBidirectionalTopologicalSort())
            .FoldM(Map<string, (Instant Es, Instant Ef)>(), (acc, id) =>
                toSeq(graph.InEdges(id))
                    .TraverseM(edge => Shift(edge.Tag, acc, duration, calendars)).As()
                    .Bind(gates => {
                        var es = gates.Max(projectStart);
                        return calendars[id].Advance(es, duration[id]).Map(ef => acc.Add(id, (es, ef)));
                    })).As();

    static Fin<Map<string, (Instant Ls, Instant Lf)>> Backward(
        BidirectionalGraph<string, STaggedEdge<string, SequenceRel>> graph, Map<string, WorkCalendar> calendars,
        Map<string, Duration> duration, Instant projectFinish) =>
        toSeq(graph.SourceFirstBidirectionalTopologicalSort(TopologicalSortDirection.Backward))
            .FoldM(Map<string, (Instant Ls, Instant Lf)>(), (acc, id) =>
                toSeq(graph.OutEdges(id))
                    .TraverseM(edge => BackShift(edge.Tag, acc, duration, calendars)).As()
                    .Bind(gates => {
                        var lf = gates.Min(projectFinish);
                        return calendars[id].Recede(lf, duration[id]).Map(ls => acc.Add(id, (ls, lf)));
                    })).As();

    static Fin<Map<string, CriticalPath>> Paths(
        BidirectionalGraph<string, STaggedEdge<string, SequenceRel>> graph, ScheduleNetwork network,
        Map<string, WorkCalendar> calendars, Map<string, (Instant Es, Instant Ef)> forward,
        Map<string, (Instant Ls, Instant Lf)> backward, Map<string, Duration> duration) =>
        network.Tasks.FoldM(Map<string, CriticalPath>(), (acc, task) =>
            toSeq(graph.OutEdges(task.GlobalId))
                .TraverseM(edge => Shift(edge.Tag, forward, duration, calendars)
                    .Map(demand => forward[edge.Target].Es - demand)).As()
                .Map(slack => {
                    var (es, ef) = forward[task.GlobalId];
                    var (ls, lf) = backward[task.GlobalId];
                    var free = slack.Head.Match(
                        Some: head => slack.Fold(head, static (min, d) => d < min ? d : min),
                        None: () => Duration.Zero);
                    return acc.Add(task.GlobalId, CriticalPath.Of(es, ef, ls, lf, free));
                })).As();

    static Fin<Instant> Shift(SequenceRel edge, Map<string, (Instant Es, Instant Ef)> forward, Map<string, Duration> duration, Map<string, WorkCalendar> calendars) {
        var (es, ef) = forward[edge.PredecessorGlobalId];
        var calendar = calendars[edge.SuccessorGlobalId];
        var lagged = ((edge.Kind.FromFinish ? ef : es).InZone(calendar.Zone).LocalDateTime + edge.Lag).InZoneLeniently(calendar.Zone).ToInstant();
        return edge.Kind.ToFinish ? calendar.Recede(lagged, duration[edge.SuccessorGlobalId]) : Fin.Succ(lagged);
    }

    static Fin<Instant> BackShift(SequenceRel edge, Map<string, (Instant Ls, Instant Lf)> backward, Map<string, Duration> duration, Map<string, WorkCalendar> calendars) {
        var (ls, lf) = backward[edge.SuccessorGlobalId];
        var calendar = calendars[edge.PredecessorGlobalId];
        var bound = ((edge.Kind.ToFinish ? lf : ls).InZone(calendar.Zone).LocalDateTime - edge.Lag).InZoneLeniently(calendar.Zone).ToInstant();
        return edge.Kind.FromFinish ? Fin.Succ(bound) : calendar.Advance(bound, duration[edge.PredecessorGlobalId]);
    }

    static Fin<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>> Graph(ScheduleNetwork network) {
        var taskIds = toHashSet(network.Tasks.Map(static t => t.GlobalId));
        return network.Dependencies
            .Find(d => !taskIds.Contains(d.PredecessorGlobalId) || !taskIds.Contains(d.SuccessorGlobalId))
            .Match(
                Some: edge => Fin.Fail<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>>(
                    new BimFault.Refused(BimScope.Planning, BimReason.DanglingReference, string.Join(':', new object?[] { "schedule-dependency-absent-process", edge.PredecessorGlobalId, edge.SuccessorGlobalId }))),
                None: () => {
                    var graph = new BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>(allowParallelEdges: true);
                    graph.AddVertexRange(network.Tasks.Map(static t => t.GlobalId));
                    network.Dependencies.Iter(d => graph.AddEdge(new STaggedEdge<string, SequenceRel>(d.PredecessorGlobalId, d.SuccessorGlobalId, d)));
                    return graph.IsDirectedAcyclicGraph()
                        ? Fin.Succ(graph)
                        : Fin.Fail<BidirectionalGraph<string, STaggedEdge<string, SequenceRel>>>(new BimFault.Refused(BimScope.Planning, BimReason.Rejected, string.Join(':', new object?[] { "schedule-cyclic-dependency" })));
                });
    }
}
```

## [04]-[RESEARCH]

(none)
