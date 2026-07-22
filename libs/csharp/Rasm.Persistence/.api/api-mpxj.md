# [RASM_PERSISTENCE_API_MPXJ]

`MPXJ.Net` is the project-schedule interchange codec: it reads and writes ~20 scheduling-tool
formats — Primavera P6 (`XER`/`PMXML`/database), Microsoft Project (`MPP`/`MPT`/`MPX`/`MSPDI`),
Asta Powerproject, Phoenix, GanttProject, Planner, SureTrak/P3, Synchro, and more — and
materializes them all into ONE neutral `ProjectFile` object graph: a task hierarchy
(`ProjectFile.Tasks` / `ChildTasks`), a typed predecessor/successor relationship network
(`Task.Predecessors`/`Successors` → `Relation` with `RelationType` + `Lag`), resources +
assignments, working-time calendars (`ProjectCalendar` with exceptions/work-weeks), and project
properties (data/status/finish dates, schedule-from). It is the .NET projection of the Java MPXJ
library: the Java jar is IKVM-translated to IL at build, so the surface is the IKVM-proxied
`MPXJ.Net.*` namespace over the original `net.sf.mpxj` classes. This is the Persistence
schedule-FILE ingress/egress lane — the peer of the IFC/glTF/Arrow/CSV codecs for the planning
domain — feeding the parsed activity/relationship/calendar/resource graph downstream to the 4D
`ConstructionTask` schedule and the 5D `CostItem` network; it owns NO scheduling MATH (CPM/leveling
is the consumer's), only the parse/serialize round-trip.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MPXJ.Net`
- package: `MPXJ.Net`
- license: `LGPL-2.1-or-later` (weak copyleft — dynamic-link `PackageReference`, separate assembly, no source contamination)
- assembly: `MPXJ.Net` (multi-target `net6.0` + `net472`; the `lib/net6.0/MPXJ.Net.dll` is the asset the `net10.0` consumer binds — `net472` is the legacy fallback)
- namespace: `MPXJ.Net` (IKVM-proxied over the Java `net.sf.mpxj.*`)
- deps: `IKVM.Maven.Sdk` (build-time Java→IL translation of the MPXJ jar over osx-arm64), `Portable.System.DateTimeOnly` (the `DateOnly`/`TimeOnly` facade — a net10 in-box no-op, floor-pinned `9.0.1`)
- runtime: pure-managed AFTER the IKVM IL translation; the Java runtime is statically translated, NOT a JVM dependency at run time — osx-arm64-clean
- owner: `libs/csharp/Rasm.Persistence/Rasm.Persistence.csproj`
- rail: schedule-file interchange (`Ingest/schedule`)

## [02]-[OBJECT_GRAPH]

[OBJECT_GRAPH_SCOPE]: the neutral `ProjectFile` model every format parses into — the activity/relationship/resource/calendar graph
- rail: schedule-model

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]                     | [CAPABILITY]                                                 |
| :-----: | :------------------------- | :-------------------------------- | :----------------------------------------------------------- |
|  [01]   | `ProjectFile`              | `class`                           | the root container (members in `[MEMBERS]`)                  |
|  [02]   | `Task`                     | `class`                           | one activity/WBS node (members in `[MEMBERS]`)               |
|  [03]   | `Relation`                 | `class`                           | a typed dependency edge (members in `[MEMBERS]`)             |
|  [04]   | `RelationType`             | `enum`                            | the four CPM dependency kinds (values in `[MEMBERS]`)        |
|  [05]   | `Resource`                 | `class`                           | a labour/material/cost resource (members in `[MEMBERS]`)     |
|  [06]   | `ResourceAssignment`       | `class`                           | a task↔resource allocation (units, work, cost)               |
|  [07]   | `ProjectCalendar`          | `class`                           | working-time calendar (members in `[MEMBERS]`)               |
|  [08]   | `Duration`                 | `class`                           | unit-tagged magnitude + `TimeUnit?` (members in `[MEMBERS]`) |
|  [09]   | `ProjectProperties`        | `class`                           | project-level dates/schedule props (members in `[MEMBERS]`)  |
|  [10]   | `TaskContainer`            | `class : IProjectEntityContainer` | the `ProjectFile.Tasks` keyed collection                     |
|  [11]   | `ResourceContainer`        | `class : IProjectEntityContainer` | the `ProjectFile.Resources` keyed collection                 |
|  [12]   | `RelationContainer`        | `class : IProjectEntityContainer` | the `ProjectFile.Relations` keyed collection                 |
|  [13]   | `ProjectCalendarContainer` | `class : IProjectEntityContainer` | the `ProjectFile.Calendars` keyed collection                 |
|  [14]   | `TimeUnit`                 | `enum`                            | the `Duration.Units` unit (values in `[MEMBERS]`)            |
|  [15]   | `ConstraintType`           | `enum`                            | the `Task.ConstraintType` modality (values in `[MEMBERS]`)   |
|  [16]   | `RecurringData`            | `class`                           | seed generator for recurring calendar exceptions             |
|  [17]   | `Availability`             | `class`                           | date-ranged resource-capacity row                            |

[MEMBERS]: the per-type member and enum-value rosters; durations/lags are unit-tagged, never raw days.
- [01]-[PROJECTFILE]: `Tasks`/`Resources`/`Relations`/`Calendars`/`ResourceAssignments`/`ProjectProperties` + `ChildTasks` (hierarchy root)
- [02]-[TASK]: start/finish/duration/%complete/critical/baselines + early/late/actual dates, WBS/activity-id, milestone/summary, work/cost, `Predecessors`/`Successors` (`IList<Relation>`), `ChildTasks` (`IList<Task>`), `ResourceAssignments` (`IList<ResourceAssignment>`), `TotalSlack`/`FreeSlack`/`PlannedDuration` (`Duration`), `ConstraintType` (`ConstraintType?`) + `ConstraintDate` (`DateTime?`)
- [03]-[RELATION]: `PredecessorTask`/`SuccessorTask`, `Type` (`RelationType?`), `Lag` (`Duration`); a `Builder(ProjectFile)` mints one
- [04]-[RELATIONTYPE]: `FinishStart` / `StartStart` / `FinishFinish` / `StartFinish`
- [05]-[RESOURCE]: cost rates, `Availability` (`AvailabilityTable`), calendar, group
- [07]-[PROJECTCALENDAR]: `WorkWeeks` (`ProjectCalendarWeek`), `CalendarExceptions` (`ProjectCalendarException`), `CalendarType`
- [08]-[DURATION]: `DurationValue` (`double`) + `TimeUnit?` (`Units`); static `GetInstance(double\|int magnitude, TimeUnit type)` mints one when synthesizing
- [09]-[PROJECTPROPERTIES]: start/finish/status dates, `ScheduleFrom`, default calendar, currency, title; `FileType` (`string`, the parsed source-format name) + `FileApplication`
- [14]-[TIMEUNIT]: `Minutes`/`Hours`/`Days`/`Weeks`/`Months`/`Years`/`Percent` + the `Elapsed{Minutes,Hours,Days,Weeks,Months,Years,Percent}` variants
- [15]-[CONSTRAINTTYPE]: `AsSoonAsPossible`/`AsLateAsPossible`/`MustStartOn`/`MustFinishOn`/`StartNoEarlierThan`/`StartNoLaterThan`/`FinishNoEarlierThan`/`FinishNoLaterThan`/`StartOn`/`FinishOn`
- [16]-[RECURRINGDATA]: daily/weekly/monthly/yearly type, start/finish, occurrences, end-date stance, working-day/relative flags, frequency, day/month selectors, and weekly-day set/get
- [17]-[AVAILABILITY]: `Availability(DateTime? start, DateTime? end, double? units)`; `Range` exposes `DateTimeRange.Start`/`End`, and `Units` preserves the capacity magnitude

`ProjectFile` is the boundary type the Persistence schedule lane maps to its canonical `ConstructionTask`/`CostItem` shapes: walk `ChildTasks` for the WBS hierarchy, read each `Task`'s `Predecessors`/`Successors` for the dependency network, and key durations/lags through their `TimeUnit`. IKVM proxy types carry a `JavaObject` handle and an `IHasJavaObject`/`IJavaObjectProxy<T>` shape; boundary projection reads them as ordinary .NET objects and never threads the Java handle into canonical code.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: the read (auto-detect + per-format) and write (format-targeted) surfaces; reader entries hang off `new UniversalProjectReader()` and take `(string fileName | Stream)`
- rail: ingress / egress

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                                            |
| :-----: | :----------------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `.Read(...)` → `ProjectFile`                                 | read (auto)    | format-sniffs and dispatches to the concrete reader     |
|  [02]   | `.ReadAll(...)` → `IList<ProjectFile>`                       | read (multi)   | every project in a container (multi-project P6 XER)     |
|  [03]   | `.GetProjectReaderProxy(...)` → `IProjectReaderProxy`        | read (proxy)   | typed proxy when a reader needs config before `Read`    |
|  [04]   | `IProjectReader.Read(...)` → `ProjectFile`                   | read (typed)   | concrete per-format reader contract                     |
|  [05]   | `new UniversalProjectWriter(FileFormat).Write(...)`          | write          | serialize a `ProjectFile` to a target format            |
|  [06]   | `IProjectWriter.Write(ProjectFile, …)`                       | write (typed)  | the per-format writer contract (writers in `[DETAIL]`)  |
|  [07]   | `FileFormat`                                                 | enum           | the seven writable `FileFormat` targets                 |
|  [08]   | `Relation.Builder(ProjectFile)…Build()`                      | mutate         | construct a dependency edge when synthesizing           |
|  [09]   | `Duration.GetInstance(double\|int magnitude, TimeUnit type)` | mutate         | mint a unit-tagged `Duration` for a lag on write        |
|  [10]   | `ProjectFile.AddTask()`                                      | mutate         | mint a task child                                       |
|  [11]   | `ProjectFile.AddResource()`                                  | mutate         | mint a resource child                                   |
|  [12]   | `ProjectFile.AddCalendar()`                                  | mutate         | mint a calendar child                                   |
|  [13]   | `ProjectFile.GetTaskByUniqueID(int)`                         | mutate         | resolve a task by durable unique id                     |
|  [14]   | `Task` / `ProjectProperties` read accessors                  | read           | CPM/actuals/constraint accessors (fields in `[DETAIL]`) |
|  [15]   | `ProjectFile.GetResourceByUniqueID(int)`                     | mutate         | resolve a resource by durable unique id                 |
|  [16]   | `Task.AddTask()`                                             | mutate         | mint a WBS child under its parent                       |
|  [17]   | `Task.AddResourceAssignment(Resource)`                       | mutate         | mint one task-resource loading                          |
|  [18]   | `ProjectCalendar.AddWorkWeek()`                              | mutate         | mint a date-ranged weekly override                      |
|  [19]   | `ProjectCalendar.AddCalendarException(DateOnly[, DateOnly])` | mutate         | mint a dated override                                   |
|  [20]   | `ProjectCalendarDays.SetWorkingDay` / `AddCalendarHours`     | mutate         | rebuild a day rule and its shift ranges                 |
|  [21]   | `ProjectCalendarHours.Add(TimeOnlyRange)`                    | mutate         | append a shift to a day, week, or exception             |
|  [22]   | `ProjectFile.GetCalendarByUniqueID(int)`                     | mutate         | resolve a calendar by durable unique id                 |
|  [23]   | `ProjectCalendar.AddCalendarException(RecurringData)`        | mutate         | mint a generated recurring override                     |

[DETAIL]: writer roster, container-write overload, synthesis settables, and read accessors.
- [01]-[WRITERS]: `PrimaveraPMFileWriter`/`PrimaveraXERFileWriter`/`MSPDIWriter`/`MPXWriter`/`JsonWriter`/`PlannerWriter`/`SDEFWriter`
- [02]-[WRITE_CONTAINER]: `Write(IList<ProjectFile>, …)` serializes a multi-project container
- [03]-[SYNTHESIS]: task hierarchy, schedule/actual/baseline/constraint fields, relations, calendars, resources, and assignments are settable through rows `[08]`-`[21]`
- [04]-[READ_ACCESSORS]: `Task.TotalSlack`/`FreeSlack`/`Critical`/`ConstraintType`/`ConstraintDate`/`ActualStart`/`ActualFinish`; `ProjectProperties.FileType`/`FileApplication`/`ProjectTitle`/`ScheduleFrom`
- [05]-[TASK_WORK_COST]: `Task` exposes settable actual/baseline/planned/remaining duration, work, and cost plus budget cost, summary, and outline level
- [06]-[PROJECT_ANCHOR]: `ProjectProperties` exposes settable current/start/finish/status dates, default-calendar id, minutes per day, days per month, currency, title, and `ScheduleFrom`
- [07]-[RESOURCE_LOADING]: `Resource` exposes settable type, calendar, cost, actual cost, and overtime cost; `ResourceAssignment` exposes settable actual/remaining work and cost
- [08]-[RESOURCE_CAPACITY]: `Resource.PeakUnits` is settable; `Resource.Availability` is an `AvailabilityTable : IList<Availability>`, so dated capacity rows append through `Add`

`UniversalProjectReader.Read` is the lane's normal ingress — it auto-detects ANY supported format, so the consumer never branches on file extension. Write capability is asymmetric: MPXJ READS ~20 formats but WRITES only the seven `FileFormat` members (notably `PMXML`/`XER` for P6 round-trip and `MSPDI`/`MPX` for MS Project, plus the neutral `JSON`).

## [04]-[IMPLEMENTATION_LAW]

[FORMAT_COVERAGE]:
- READ (auto-dispatched by `UniversalProjectReader`): Primavera P6 (`PrimaveraXERFileReader` XER, `PrimaveraPMFileReader` PMXML, `PrimaveraDatabaseReader`/`P3DatabaseReader`/`P3PRXFileReader`), MS Project (`MPPReader` MPP/MPT binary, `MSPDIReader` XML, `MPXReader` MPX, `MPDFileReader`), Asta (`AstaFileReader`/`AstaMdbReader`/`AstaSqliteReader`/`AstaTextFileReader`), `PhoenixReader`, `GanttProjectReader`/`GanttDesignerReader`, `PlannerReader`/`ProjectLibreReader`, `SureTrakDatabaseReader`/`SureTrakSTXFileReader`, `SynchroReader`, `MerlinReader`, `FastTrackReader`, `TurboProjectReader`, `OpenPlanReader`, `ConceptDrawProjectReader`, `EdrawProjectReader`, `SageReader`, `SDEFReader`, `ProjectCommanderReader`, plus the web-service `PwaReader`/`OpcReader`/`WebServicesReader`
- WRITE (the seven `FileFormat` members only): `XER`/`PMXML` (P6 round-trip), `MSPDI`/`MPX` (MS Project), `PLANNER`, `SDEF`, and `JSON` (`JsonWriter` — the neutral textual dump); there is NO MPP writer (MPP is read-only, a Microsoft binary format)
- Ingress contracts through `UniversalProjectReader.Read` (one call, format-agnostic); per-format `IProjectReader` types remain the configured-reader fallback

[SCHEDULE_GRAPH]:
- Dependency network is `Task.Predecessors`/`Successors` → `IList<Relation>`, each `Relation` carrying `Type` (`RelationType`: `FinishStart`/`StartStart`/`FinishFinish`/`StartFinish`) and `Lag` (`Duration`) — this is the directed activity-on-node graph the consumer's CPM/topological-sort runs over; MPXJ supplies the graph, never the forward/backward pass
- `Task` carries the schedule fields the 4D/5D networks read (start/finish/duration, %complete, critical flag, total/free slack, early/late start/finish, baseline start/finish, actual start/finish, work, cost, milestone/summary flags, constraint type/date, WBS/activity-id, outline level) — dates surface as the BCL date types via the `Portable.System.DateTimeOnly` facade
- WBS hierarchy is `ProjectFile.ChildTasks` (roots) → each `Task`'s child tasks; `ProjectFile.Tasks` is the flat keyed container
- `Duration` is unit-tagged (`Units` is a `TimeUnit?`): a duration/lag is a magnitude in days/hours/weeks/etc., so the consumer converts through the unit rather than assuming days — this meets the canonical `NodaTime`/`UnitsNet` time vocabulary at the boundary

[CALENDAR_RESOURCE]:
- `ProjectCalendar` defines working time: `WorkWeeks` (`ProjectCalendarWeek` — per-day work hours) and `CalendarExceptions` (`ProjectCalendarException` — holidays/overrides), keyed by `CalendarType`; the consumer's working-day arithmetic reads these rather than assuming a 5-day week
- `Resource` (cost rates, `Availability` → `AvailabilityTable` time-phased units, calendar, group) + `ResourceAssignment` (task↔resource units/work/cost) carry the resource-loading the 5D cost lane and any resource-leveling consumer needs
- `ProjectProperties` anchors the schedule: start/finish/status dates, `ScheduleFrom` (forward from start vs backward from finish), default calendar, currency

[IKVM_BOUNDARY]:
- IKVM-translated surface projects `net.sf.mpxj.*` through `MPXJ.Net.*`; `_proxyManager` lazily wraps Java collections as `IList<T>`, and each proxy carries a `JavaObject` handle behind `IHasJavaObject`/`IJavaObjectProxy<T>`
- Boundary projection reads ordinary .NET objects and maps `ProjectFile` to canonical `ConstructionTask`/`CostItem` shapes at ONE seam — the Java handle never threads into canonical code, and proxy types are not re-exposed
- LGPL-2.1 is satisfied by dynamic-link `PackageReference` (separate assembly): the codec is consumed, never statically fused into a Rasm assembly, and never re-published — the same posture as the other weak-copyleft floors

[INTEGRATION_STACK]:
- `Ingest/schedule#SCHEDULE_INGRESS` is the consumer: `UniversalProjectReader.Read(bytes/path)` → `ProjectFile`, folded into the canonical schedule model; the parse runs inside the codec read under the Persistence `IO`/`Fin` rail, a malformed/unsupported file surfacing as a typed parse rejection
- Parsed `Task`/`Relation` network projects to Bim's 4D `ConstructionTask` (`Rasm.Bim` `Planning/schedule.md`) — the activity-on-node graph + `RelationType`/`Lag` edges feed the 4D sequencing, the dependency edges being exactly the `SequenceRel` DAG `Rasm.Bim`'s `QuikGraph` (`api-quikgraph`) runs `SourceFirstTopologicalSort` over for the CPM activity order (MPXJ supplies the edges, QuikGraph the order, the `WorkCalendar` fold the float/calendar arithmetic) — and the resource/cost loading (`ResourceAssignment.Cost`/`.BudgetCost`/`.Units`/`.Work` and `Resource.StandardRate`/`.Cost`/`.CostPerUse`, all raw `double?`/`Rate` off the parse) projects to the 5D `CostItem` network where each foreign `double` is lifted into a `Money` at the boundary by `Rasm.Bim`'s `NodaMoney` (`api-nodamoney`, peer to its IFC `IfcCostValue` cost-graph lift) and the dimensioned quantity by `UnitsNet` (`Money * (decimal)quantity` for cost × quantity)
- this is the schedule-FILE peer of the other Persistence interchange codecs: it sits in `[WIRE_SERIALIZATION]` beside `MessagePack`/`Sep` and complements the row-oriented (`Sep`/`MiniExcel`) and columnar (Arrow/Parquet) lanes — those cannot parse a binary MPP or a P6 XER, MPXJ owns exactly that schedule-tool format space
- durations/dates meet `NodaTime` (the clock seam) and `UnitsNet` (the quantity substrate) at the boundary through `Duration.Units`; the schedule graph meets `QuikGraph` for the CPM topological order

[LOCAL_ADMISSION]:
- ingress is `UniversalProjectReader.Read` (auto-detect); the lane does NOT branch on file extension and does NOT hand-roll an XER/MPX/MSPDI parser
- `ProjectFile` is the boundary type mapped ONCE to the canonical schedule shapes; the IKVM proxy types and the Java handle never thread into canonical code
- durations and lags are read through their `TimeUnit`, never assumed to be days; working-day arithmetic reads `ProjectCalendar`, never assumes a 5-day week
- writes target the seven `FileFormat` members only (no MPP writer exists); the codec is consumed under LGPL dynamic-link and never statically fused or re-published
- scheduling MATH (CPM forward/backward pass, resource leveling, 4D sequencing) is the CONSUMER's (`QuikGraph` + the canonical schedule model); MPXJ owns parse/serialize only

[RAIL_LAW]:
- Package: `MPXJ.Net`
- Owns: read of ~20 project-schedule file formats (P6 XER/PMXML, MS Project MPP/MSPDI/MPX, Asta, Phoenix, …) into a neutral `ProjectFile` graph, and write to seven (`JSON`/`MPX`/`MSPDI`/`PLANNER`/`PMXML`/`XER`/`SDEF`)
- Accept: a schedule file/stream (any supported format) for read; a `ProjectFile` + `FileFormat` for write
- Reject: a hand-rolled XER/MPX/MSPDI/MPP parser; branching the ingress on file extension instead of `UniversalProjectReader`; threading the IKVM Java handle into canonical code; computing CPM/leveling inside the codec (that is the consumer's `QuikGraph` + schedule model); a second schedule-file codec beside this one; static-linking or re-publishing the LGPL assembly
