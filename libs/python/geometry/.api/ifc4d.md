# [PY_GEOMETRY_API_IFC4D]

`ifc4d` owns the IFC 4D construction-scheduling round-trip for the geometry ifc-analysis rail: one `<Format>2Ifc` parser class per external scheduling format populates an `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree over the `ifcopenshell` model, and the symmetric `Ifc2<Format>` writer exports a schedule back out. `IfcLifecycle` consumes it at its `SCHEDULE` phase, keyed by the `ScheduleFormat` parser vocabulary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc4d`
- package: `ifc4d` (LGPL-3.0-or-later, IfcOpenShell)
- module: `ifc4d.<name>2ifc`, one parser module per format (`import ifc4d.msp2ifc`)
- rail: ifc-analysis / 4d-scheduling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schedule parser and writer classes — each imports from `ifc4d.<lowercased-class>` (`MSP2Ifc` from `ifc4d.msp2ifc`); the format is the named class, never a parse-per-format function on one entry.

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY] | [CAPABILITY]                   |
| :-----: | :---------- | :------------ | :----------------------------- |
|  [01]   | `MSP2Ifc`   | class         | MS Project XML to IFC schedule |
|  [02]   | `P62Ifc`    | class         | Primavera P6 XML to schedule   |
|  [03]   | `P6XER2Ifc` | class         | Primavera P6 XER to schedule   |
|  [04]   | `PP2Ifc`    | class         | Asta Powerproject to schedule  |
|  [05]   | `Ifc2P6`    | class         | IFC schedule out to P6 XML     |
|  [06]   | `Ifc2Msp`   | class         | IFC schedule out to MS Project |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: every class shares one call shape — construct, assign slots, `execute()`; no positional argument carries input, and `.file`/`.xml` reverse meaning between parser and writer.

| [INDEX] | [SURFACE]                  | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------- | :------- | :----------------------------------------- |
|  [01]   | `<Format>2Ifc().execute()` | instance | populate the IFC task tree from the source |
|  [02]   | `Ifc2<Format>().execute()` | instance | write the `.file` schedule out to `.xml`   |

[SLOTS]: assigned before `execute()`, the mutable input contract.
- `.file`: `ifcopenshell.file` — parser target model, or writer source model.
- source: `.xml` (`MSP2Ifc`, `P62Ifc`, writers), `.xer` (`P6XER2Ifc`), `.pp` (`PP2Ifc`).
- `.work_plan`: `IfcWorkPlan | None` parent; `.output`: P6/XER/PP and writer export path; `Ifc2Msp` also sets `.work_schedule`.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Import each parser module function-local at boundary scope (`import ifc4d.msp2ifc`); the `ifcopenshell` native build gates module load.
- A new scheduling format is one `<Format>2Ifc` parser class, never a parse-per-format function on one entry; CSV round-trip rides `csv4d2ifc.Csv2Ifc` for task schedules and `csv2ifc.Csv2Ifc`/`Ifc2Csv` for resources.
- 5D cost stays `ifc5d`, model transform `ifcpatch`, revision diff `ifcdiff`, IFC parse and tessellation `ifcopenshell`.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): the `.file` slot binds an `ifcopenshell.file`, `execute()` populates `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` into it, and each populated `IfcTask` GlobalId reads back through `ifcopenshell` entity access.
- `IfcLifecycle`: its `SCHEDULE` phase folds the `ScheduleFormat` discriminant onto the `<Format>2Ifc` class, assigns `.file`/source/`.work_plan`, runs `execute()`, then projects each `IfcTask` GlobalId as a `LifecycleRow.of_task` row.

[LOCAL_ADMISSION]:
- Each scheduling format is admitted as its `<Format>2Ifc` parser row; the class owns the XML/XER/Powerproject decode into the task tree.

[RAIL_LAW]:
- Package: `ifc4d`
- Owns: 4D construction-schedule round-trip between IFC `IfcWorkSchedule`/`IfcTask` and MS Project / Primavera P6 (XML, XER) / Asta Powerproject, and task-tree authoring.
- Accept: a source schedule file with a target `ifcopenshell.file`, feeding the `IfcLifecycle` `SCHEDULE` phase.
- Reject: a hand-rolled P6/MS Project XML parser; a parse-per-format function family where the `<Format>2Ifc` class owns the conversion.
