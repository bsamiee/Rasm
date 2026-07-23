# [RASM_PERSISTENCE_API_MPXJ]

`MPXJ.Net` owns project-schedule file interchange: every scheduling-tool dialect it reads materializes into one neutral `ProjectFile` graph — WBS hierarchy, typed dependency edges, unit-tagged durations, working-time calendars, resource loading — and `FileFormat` names every dialect it writes back. `ProjectCalendar` owns the working-time arithmetic and `Duration` the unit conversion, so a consumer resolves both on the surface. Scheduling math — the CPM passes, float, leveling — is the consumer's; this codec owns the round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MPXJ.Net`
- package: `MPXJ.Net` (LGPL-2.1-or-later)
- assembly: `MPXJ.Net`, the `lib/net6.0` asset a `net10.0` consumer binds
- namespace: `MPXJ.Net`, IKVM-proxied over the Java `net.sf.mpxj`
- abi: pure managed at run time — `IKVM.Maven.Sdk` translates the MPXJ jar to IL inside the consuming build, and no JVM loads after it
- depends: `IKVM.Maven.Sdk`, `Portable.System.DateTimeOnly`
- rail: schedule-file interchange

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the neutral `ProjectFile` graph, its calendar and economic rows, and the enums those rows discriminate on

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                                  |
| :-----: | :------------------------- | :------------- | :-------------------------------------------- |
|  [01]   | `ProjectFile`              | class          | root graph, container roster, baseline set    |
|  [02]   | `Task`                     | class          | one activity or WBS node                      |
|  [03]   | `Relation`                 | class          | one typed dependency edge                     |
|  [04]   | `Relation.Builder`         | class          | fluent edge mint bound to a file              |
|  [05]   | `RelationType`             | enum           | CPM dependency modality                       |
|  [06]   | `ConstraintType`           | enum           | activity date-constraint modality             |
|  [07]   | `TaskType`                 | enum           | fixed units, duration, or work scheduling     |
|  [08]   | `TaskMode`                 | enum           | auto or manual scheduling stance              |
|  [09]   | `ActivityType`             | enum           | P6 activity classification                    |
|  [10]   | `ActivityStatus`           | enum           | P6 activity progress state                    |
|  [11]   | `PercentCompleteType`      | enum           | which measure drives percent complete         |
|  [12]   | `EarnedValueMethod`        | enum           | earned-value accrual rule                     |
|  [13]   | `AccrueType`               | enum           | cost accrual timing                           |
|  [14]   | `Resource`                 | class          | one labour, material, or cost resource        |
|  [15]   | `ResourceType`             | enum           | resource kind vocabulary                      |
|  [16]   | `ResourceAssignment`       | class          | one task-resource loading row                 |
|  [17]   | `Availability`             | class          | one dated capacity row                        |
|  [18]   | `AvailabilityTable`        | class          | date-indexed capacity rows                    |
|  [19]   | `CostRateTable`            | class          | dated rate schedule for one resource          |
|  [20]   | `CostRateTableEntry`       | class          | one dated rate band                           |
|  [21]   | `Rate`                     | class          | amount-per-time-unit rate value               |
|  [22]   | `WorkContour`              | class          | work distribution curve over an assignment    |
|  [23]   | `ProjectCalendar`          | class          | working-time calendar and its arithmetic      |
|  [24]   | `ProjectCalendarDays`      | abstract class | per-weekday day types and shift sets          |
|  [25]   | `ProjectCalendarWeek`      | class          | date-ranged weekly override                   |
|  [26]   | `ProjectCalendarException` | class          | dated or recurring day override               |
|  [27]   | `ProjectCalendarHours`     | class          | shift ranges for one day                      |
|  [28]   | `RecurringData`            | class          | recurrence seed for a calendar exception      |
|  [29]   | `RecurrenceType`           | enum           | daily, weekly, monthly, or yearly recurrence  |
|  [30]   | `CalendarType`             | enum           | global, project, or resource calendar         |
|  [31]   | `DayType`                  | enum           | working, non-working, or inherited day        |
|  [32]   | `Duration`                 | class          | unit-tagged magnitude with conversion algebra |
|  [33]   | `TimeUnit`                 | enum           | duration unit vocabulary, elapsed rows too    |
|  [34]   | `ProjectProperties`        | class          | project anchors, defaults, file provenance    |
|  [35]   | `ScheduleFrom`             | enum           | forward from start or backward from finish    |
|  [36]   | `FileFormat`               | enum           | closed write-target vocabulary                |
|  [37]   | `ActivityCode`             | class          | one P6 code dimension                         |
|  [38]   | `ActivityCodeValue`        | class          | one value inside a code dimension             |
|  [39]   | `Step`                     | class          | one activity step row                         |
|  [40]   | `ExpenseItem`              | class          | one activity expense row                      |
|  [41]   | `ProjectEntityContainer`   | class          | keyed collection base, durable-id resolve     |

Every duration and lag carries its `TimeUnit`; a bare magnitude has no meaning without it. Closed enum vocabularies lead the roster block, per-type field sets follow.

[RELATIONTYPE]: `FinishFinish` `FinishStart` `StartFinish` `StartStart`

[CONSTRAINTTYPE]: `AsSoonAsPossible` `AsLateAsPossible` `MustStartOn` `MustFinishOn` `StartNoEarlierThan` `StartNoLaterThan` `FinishNoEarlierThan` `FinishNoLaterThan` `StartOn` `FinishOn`

[TIMEUNIT]: `Minutes` `Hours` `Days` `Weeks` `Months` `Percent` `Years` `ElapsedMinutes` `ElapsedHours` `ElapsedDays` `ElapsedWeeks` `ElapsedMonths` `ElapsedYears` `ElapsedPercent`

[FILEFORMAT]: `JSON` `MPX` `MSPDI` `PLANNER` `PMXML` `XER` `SDEF`

[TASKTYPE]: `FixedUnits` `FixedDuration` `FixedWork` `FixedDurationAndUnits`

[ACTIVITYTYPE]: `TaskDependent` `ResourceDependent` `LevelOfEffort` `StartMilestone` `FinishMilestone` `WbsSummary` `Hammock` `StartFlag` `FinishFlag`

[ACTIVITYSTATUS]: `NotStarted` `InProgress` `Completed`

[PERCENTCOMPLETETYPE]: `Duration` `Physical` `Units` `Scope`

[EARNEDVALUEMETHOD]: `PercentComplete` `PhysicalPercentComplete`

[RESOURCETYPE]: `Material` `Work` `Cost` `NonLabor`

[ACCRUETYPE]: `Start` `End` `Prorated`

[SCHEDULEFROM]: `Start` `Finish`

[CALENDARTYPE]: `Global` `Project` `Resource`

[DAYTYPE]: `NonWorking` `Working` `Default`

[RECURRENCETYPE]: `Daily` `Weekly` `Monthly` `Yearly`

[TASKMODE]: `ManuallyScheduled` `AutoScheduled`

[PROJECTFILE]: `Tasks` `ChildTasks` `Resources` `ChildResources` `Relations` `Calendars` `CalendarsForProject` `DefaultCalendar` `BaselineCalendar` `Baselines` `ResourceAssignments` `ProjectProperties` `ActivityCodes` `UserDefinedFields` `ExpenseCategories` `CostAccounts` `WorkContours` `Locations` `UnitsOfMeasure` `Currencies` `Shifts` `EarliestStartDate` `LatestFinishDate` `PopulatedFields`

[TASK_SCHEDULE]: `Start` `Finish` `Duration` `EarlyStart` `EarlyFinish` `LateStart` `LateFinish` `TotalSlack` `FreeSlack` `StartSlack` `FinishSlack` `Critical` `LongestPath` `Milestone` `Summary` `Active` `Deadline` `ConstraintType` `ConstraintDate` `SecondaryConstraintType` `SecondaryConstraintDate` `Type` `TaskMode` `Calendar`

[TASK_PROGRESS]: `ActualStart` `ActualFinish` `ActualDuration` `RemainingDuration` `PercentageComplete` `PhysicalPercentComplete` `PercentCompleteType` `ActivityStatus` `CompleteThrough` `SuspendDate` `Resume`

[TASK_BASELINE]: `BaselineStart` `BaselineFinish` `BaselineDuration` `BaselineWork` `BaselineCost` `PlannedStart` `PlannedFinish` `PlannedDuration` `PlannedWork` `PlannedCost` `ScheduledStart` `ScheduledFinish` `ScheduledDuration`

[TASK_ECONOMICS]: `Work` `RegularWork` `OvertimeWork` `RemainingWork` `ActualWork` `Cost` `FixedCost` `FixedCostAccrual` `BudgetCost` `BudgetWork` `ActualCost` `RemainingCost` `BCWP` `BCWS` `ACWP` `CV` `SV` `EarnedValueMethod` `ExpenseItems`

[TASK_STRUCTURE]: `UniqueID` `ID` `WBS` `ActivityID` `CanonicalActivityID` `ActivityType` `OutlineLevel` `OutlineNumber` `SequenceNumber` `ChildTasks` `ParentTask` `ParentTaskUniqueID` `Predecessors` `Successors` `ResourceAssignments` `PrimaryResource` `ActivityCodeValues` `Steps` `Location` `Notes` `GUID`

[RELATION]: `PredecessorTask` `SuccessorTask` `Type` `Lag` `Driving` `UniqueID`

[RESOURCE]: `Type` `MaxUnits` `PeakUnits` `Availability` `AvailableFrom` `AvailableTo` `CanLevel` `OverAllocated` `Calendar` `Group` `StandardRate` `OvertimeRate` `CostPerUse` `Cost` `ActualCost` `OvertimeCost` `RemainingCost` `AccrueAt` `Work` `ActualWork` `RemainingWork` `BCWP` `BCWS` `ACWP` `CV` `SV` `Budget` `Generic` `Active` `ParentResource` `TaskAssignments` `UniqueID`

[RESOURCEASSIGNMENT]: `Units` `RemainingUnits` `Work` `ActualWork` `RemainingWork` `OvertimeWork` `Cost` `BudgetCost` `BudgetWork` `ActualCost` `RemainingCost` `Start` `Finish` `Delay` `LevelingDelay` `WorkContour` `EffectiveCalendar` `OverrideRate` `RateSource` `RateIndex` `Role` `CostRateTable` `VAC` `BCWP` `BCWS` `ACWP` `Task` `Resource` `UniqueID`

[PROJECTCALENDAR]: `WorkWeeks` `CalendarExceptions` `ExpandedCalendarExceptions` `CalendarHours` `CalendarDayTypes` `Type` `Parent` `DerivedCalendars` `Default` `Personal` `MinutesPerDay` `MinutesPerWeek` `MinutesPerMonth` `MinutesPerYear` `DaysPerMonth` `Tasks` `Resources` `UniqueID`

[PROJECTPROPERTIES]: `StartDate` `FinishDate` `CurrentDate` `StatusDate` `ScheduleFrom` `DefaultCalendar` `ActivityDefaultCalendar` `DefaultDurationUnits` `DefaultWorkUnits` `DefaultStandardRate` `DefaultTaskType` `MinutesPerDay` `MinutesPerWeek` `DaysPerMonth` `WeekStartDay` `FiscalYearStartMonth` `CriticalSlackLimit` `MultipleCriticalPaths` `HonorConstraints` `CurrencyCode` `CurrencySymbol` `CurrencyDigits` `ProjectTitle` `ProjectID` `Company` `Manager` `Author` `LastSaved` `CreationDate` `FileType` `FileApplication` `CustomProperties` `BaselineTypeName` `LastBaselineUpdateDate`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the format-sniffing read, the format-targeted write, the calendar arithmetic, and the synthesis surface a written graph is built through

| [INDEX] | [SURFACE]                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :--------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `UniversalProjectReader()`                                       | ctor     | bind the format-sniffing reader          |
|  [02]   | `AbstractProjectReader.ReadAll(string\|Stream)`                  | instance | every project in a container             |
|  [03]   | `AbstractProjectReader.Read(string\|Stream) -> ProjectFile`      | instance | the container's first project alone      |
|  [04]   | `UniversalProjectReader.GetProjectReaderProxy(Stream)`           | instance | reach the concrete reader before it runs |
|  [05]   | `UniversalProjectWriter(FileFormat)`                             | ctor     | bind a write target                      |
|  [06]   | `IProjectWriter.Write(ProjectFile, Stream)`                      | instance | serialize one project                    |
|  [07]   | `IProjectWriter.Write(IList<ProjectFile>, Stream)`               | instance | serialize a multi-project container      |
|  [08]   | `JsonWriter.WriteTimephasedData`                                 | property | emit time-phased curves in the dump      |
|  [09]   | `MSPDIWriter.SaveVersion`                                        | property | pick the MS Project schema level         |
|  [10]   | `PrimaveraPMFileWriter.WriteBaselines`                           | property | carry baselines into PMXML               |
|  [11]   | `ProjectFile.AddTask() -> Task`                                  | instance | mint a root activity                     |
|  [12]   | `Task.AddTask() -> Task`                                         | instance | mint a WBS child                         |
|  [13]   | `Relation.Builder(ProjectFile)`                                  | ctor     | open an edge build                       |
|  [14]   | `Task.AddPredecessor(Relation.Builder) -> Relation`              | instance | close and attach a dependency edge       |
|  [15]   | `Duration.GetInstance(double, TimeUnit) -> Duration`             | static   | mint a unit-tagged magnitude             |
|  [16]   | `Duration.ConvertUnits(TimeUnit, ITimeUnitDefaultsContainer<T>)` | instance | restate a magnitude in another unit      |
|  [17]   | `ProjectFile.AddCalendar() -> ProjectCalendar`                   | instance | mint a calendar                          |
|  [18]   | `ProjectCalendar.AddWorkWeek() -> ProjectCalendarWeek`           | instance | mint a date-ranged weekly override       |
|  [19]   | `ProjectCalendar.AddCalendarException(DateOnly, DateOnly)`       | instance | mint a dated override span               |
|  [20]   | `ProjectCalendar.AddCalendarException(RecurringData)`            | instance | mint a recurring override                |
|  [21]   | `ProjectCalendarDays.SetWorkingDay(DayOfWeek, bool)`             | instance | set one weekday's day type               |
|  [22]   | `ProjectCalendarDays.AddCalendarHours(DayOfWeek)`                | instance | open one weekday's shift set             |
|  [23]   | `ProjectCalendarHours.Add(TimeOnlyRange)`                        | instance | append one shift range                   |
|  [24]   | `ProjectCalendar.GetWork(DateTime, DateTime, TimeUnit)`          | instance | working content between two stamps       |
|  [25]   | `ProjectCalendar.GetDate(DateTime, Duration) -> DateTime?`       | instance | advance a stamp by working time          |
|  [26]   | `ProjectCalendar.GetNextWorkStart(DateTime) -> DateTime?`        | instance | next working moment                      |
|  [27]   | `ProjectCalendar.IsWorkingDate(DateOnly) -> bool`                | instance | working-day predicate                    |
|  [28]   | `ProjectCalendar.GetHours(DateOnly) -> ProjectCalendarHours`     | instance | shifts effective on one date             |
|  [29]   | `ProjectFile.AddResource() -> Resource`                          | instance | mint a resource                          |
|  [30]   | `Task.AddResourceAssignment(Resource) -> ResourceAssignment`     | instance | mint one loading row                     |
|  [31]   | `Resource.GetTimephasedWork(IList<DateTimeRange>, TimeUnit)`     | instance | time-phased work curve                   |
|  [32]   | `AvailabilityTable.GetEntryByDate(DateTime) -> Availability`     | instance | capacity effective at a date             |
|  [33]   | `ProjectEntityContainer.GetByUniqueID(int?)`                     | instance | durable-id resolve on any container      |
|  [34]   | `ProjectFile.GetTaskByUniqueID(int) -> Task`                     | instance | durable-id resolve, task                 |
|  [35]   | `ProjectFile.GetResourceByUniqueID(int) -> Resource`             | instance | durable-id resolve, resource             |
|  [36]   | `ProjectFile.UpdateStructure()`                                  | instance | re-derive outline levels after synthesis |
|  [37]   | `ProjectFile.SetBaseline(ProjectFile, int)`                      | instance | attach one indexed baseline snapshot     |
|  [38]   | `ProjectFile.ExpandSubprojects(bool)`                            | instance | inline external sub-project files        |
|  [39]   | `Task.GetFieldByAlias(string) -> object`                         | instance | read a custom field by its tool alias    |

- `AbstractProjectReader.Read`: yields the container's first project and drops the rest, so a multi-project XER round-trips only through `ReadAll`.
- `Duration.ConvertUnits`: both `ProjectCalendar` and `ProjectProperties` satisfy `ITimeUnitDefaultsContainer<T>`, and each carries its own minutes-per-day and days-per-month.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One neutral graph absorbs every dialect: `UniversalProjectReader` sniffs the format and dispatches to the concrete reader, so one call ingests any supported file and an upstream dialect addition costs the consumer nothing.
- Read coverage exceeds write coverage — `FileFormat` is the whole write vocabulary, and an MPP target holds no row in it.
- `[WRITERS]`: `PrimaveraPMFileWriter` `PrimaveraXERFileWriter` `MSPDIWriter` `MPXWriter` `JsonWriter` `PlannerWriter` `SDEFWriter`
- `Task.Predecessors` and `Task.Successors` project the same `Relation` set from opposite ends, so a walk reading both sides sees each edge twice.
- Every proxy carries its `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>` and otherwise reads as an ordinary .NET object.

[STACKING]:
- `Ingest/schedule#SCHEDULE_SOURCE`: sole consumer — `new UniversalProjectReader().ReadAll(string|Stream)` folds every project of a container into the durable activity, relation, calendar, and resource rows, and `Synthesis.Fold` rebuilds a `ProjectFile` through `Relation.Builder`, `Duration.GetInstance`, and the calendar day and hour surface for the `UniversalProjectWriter(FileFormat).Write` egress.
- `QuikGraph`(`libs/csharp/.api/api-quikgraph.md`): the `Relation` set IS the activity-on-node DAG, ordered by `SourceFirstBidirectionalTopologicalSort` at `Rasm.Bim/Planning/schedule#CRITICAL_PATH` before the forward and backward float pass runs over it.
- `NodaMoney`(`libs/csharp/Rasm.Bim/.api/api-nodamoney.md`): `ResourceAssignment.Cost`/`BudgetCost` and `Resource.StandardRate`/`CostPerUse` arrive as raw `double?` and `Rate`, lifted once into `Money` at the `Planning/cost#ESTIMATE` boundary.
- `NodaTime`(`libs/csharp/.api/api-nodatime.md`): BCL stamps off the parse cross into `LocalDateTime`/`LocalDate` at the row boundary, so no durable row keeps a `DateTime`.
- Within-lib calendar depth: `ProjectCalendar` computes working time rather than describing it, and it satisfies `ITimeUnitDefaultsContainer<T>`, so a lag restates in any unit against the project's own minutes-per-day and days-per-month.
- Within-lib write depth: a configured concrete writer replaces `UniversalProjectWriter` wherever the target carries a knob the format vocabulary alone cannot express.

[LOCAL_ADMISSION]:
- Ingress is `ReadAll`; a single-project file yields a one-element list, so arity reads off the yield.
- `ProjectFile` maps once onto the canonical rows, and the proxy types and their `JavaObject` handle stop at that seam.
- Durations and lags read through their `TimeUnit`, and working-day arithmetic runs on `ProjectCalendar`.
- Every consuming project inherits the `IKVM.Maven.Sdk` build chain; `Directory.Build.targets` owns the design-time guard that keeps it off Roslyn loads.

[RAIL_LAW]:
- Package: `MPXJ.Net`
- Owns: schedule-file parse and serialize between every supported dialect and the neutral `ProjectFile` graph
- Accept: a schedule file or stream for read; a `ProjectFile` graph with its `FileFormat` target for write
- Reject: a hand-rolled XER, MPX, MSPDI, or MPP parser; an extension-branched ingress; a `Read` ingress truncating a container; a Java handle threaded past the row boundary; CPM or leveling math inside the codec; a second schedule-file codec
