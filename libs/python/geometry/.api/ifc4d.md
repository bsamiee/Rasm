# [PY_GEOMETRY_API_IFC4D]

`ifc4d` supplies the IFC 4D construction-scheduling surface for the geometry ifc-analysis rail: round-trip conversion between IFC `IfcWorkSchedule`/`IfcTask` task trees and the external scheduling formats (Microsoft Project XML, Oracle Primavera P6 XML/XER, Asta Powerproject), plus task-tree authoring and duration/sequence calculation over the `ifcopenshell` model. It rides the `ifcopenshell` worker lane (`0.8.5`, depends `ifcopenshell`/`typing-extensions`), so `ifc/costing.md#LIFECYCLE` composes the `<Format>2Ifc` named parsers directly as the `SCHEDULE` phase — one `ScheduleFormat`-keyed row binding the parser class, setting the `.file`/`.xml`/`.work_plan` slots, and reading the populated `IfcTask` GlobalIds back as typed `LifecycleRow.of_task` rows — rather than re-deriving a schedule-task graph or hand-rolling a P6/MS Project XML parser.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc4d`
- package: `ifc4d`
- import: `import ifc4d.msproject2ifc` / `ifc4d.p62ifc` / `ifc4d.asta2ifc` (each `<Format>2Ifc` parser lives in its own module — there is no top-level `ifc4d` re-export of the parser classes)
- owner: `geometry`
- rail: ifc-analysis / 4d-scheduling
- installed: `0.8.5`
- license: LGPL-3.0-or-later (the IfcOpenShell-ecosystem license)
- entry points: none (library only)
- capability: parse Microsoft Project XML / Primavera P6 XML/XER / Asta Powerproject into an `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` task tree, write an IFC schedule back to MS Project / P6 XML, and author/sequence construction tasks over the `ifcopenshell` model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schedule parsers and writers
- rail: 4d-scheduling

Each parser is a `<Format>2Ifc` class in its own module; the format is the named class, never a parse-per-format function family on one entry — the scheduling-format vocabulary is the closed parser set the `ScheduleFormat` `StrEnum` selects.

| [INDEX] | [SYMBOL]             | [MODULE]              | [PACKAGE_ROLE] | [CAPABILITY]                                    |
| :-----: | :------------------- | :-------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `MSProject2Ifc`      | `ifc4d.msproject2ifc` | parser         | Microsoft Project XML into an IFC work schedule |
|  [02]   | `P62Ifc`             | `ifc4d.p62ifc`        | parser         | Primavera P6 XML/XER into an IFC work schedule  |
|  [03]   | `Asta2Ifc`           | `ifc4d.asta2ifc`      | parser         | Asta Powerproject into an IFC work schedule     |
|  [04]   | `Ifc2P6` / `Ifc2MSP` | `ifc4d.ifc2p6` / `…`  | writer         | IFC work schedule back to P6 XML / MS Project   |

[PUBLIC_TYPE_SCOPE]: parser instance slots
- rail: 4d-scheduling

The `<Format>2Ifc` parser is configured by assigning instance slots before `execute()`, never positional constructor args; the slots are the parser's mutable input contract.

| [INDEX] | [SLOT]       | [TYPE]                        | [CAPABILITY]                                     |
| :-----: | :----------- | :---------------------------- | :----------------------------------------------- |
|  [01]   | `.file`      | `ifcopenshell.file`           | the target model the task tree is populated into |
|  [02]   | `.xml`       | source path / parsed document | the schedule-source XML/XER the parser reads     |
|  [03]   | `.work_plan` | `IfcWorkPlan` or `None`       | the parent work plan the schedule attaches under |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schedule conversion
- rail: 4d-scheduling

Parser rows construct the class, set the `.file`/`.xml`/`.work_plan` slots, then call `execute()` to populate the task tree; writer rows consume an `ifcopenshell.file` plus a schedule entity.

| [INDEX] | [SURFACE]                                    | [CALL_SHAPE]            | [CAPABILITY]                                    |
| :-----: | :------------------------------------------- | :---------------------- | :---------------------------------------------- |
|  [01]   | `MSProject2Ifc`                              | slots + execute         | parse MS Project XML, populate the IFC schedule |
|  [02]   | `P62Ifc`                                     | slots + execute         | parse P6 XML/XER, populate the IFC schedule     |
|  [03]   | `Asta2Ifc`                                   | slots + execute         | parse Asta Powerproject, populate the schedule  |
|  [04]   | `Ifc2P6().execute()` / `Ifc2MSP().execute()` | model + schedule + path | export the IFC schedule to P6 XML / MS Project  |

## [04]-[IMPLEMENTATION_LAW]

[FOUR_D_TOPOLOGY]:
- import: `import ifc4d.msproject2ifc` / `import ifc4d.p62ifc` / `import ifc4d.asta2ifc` function-local at boundary scope under `# noqa: PLC0415`; module-level import is banned by the manifest import policy.
- conversion axis: each parser is a `<Format>2Ifc` class with `.file`/`.xml`/`.work_plan` slots and an `execute()` that populates `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` into the target model; the format is the named parser class, never a parse-per-format function family on one entry — the scheduling-format vocabulary is the closed parser set keyed by the `ScheduleFormat` `StrEnum`.
- writer axis: `Ifc2<Format>` is the symmetric export over the IFC schedule entity.
- lifecycle stacking: `ifc/costing.md#LIFECYCLE` is the integration owner — the `SCHEDULE` phase folds `ScheduleFormat` → `{MSPROJECT: MSProject2Ifc, P6: P62Ifc, ASTA: Asta2Ifc}[fmt]()` into one row, assigns `parser.file = model`, `parser.xml = source`, `parser.work_plan = model.by_type("IfcWorkPlan")[0] if present else None`, runs `parser.execute()`, then reads `model.by_type("IfcTask")` GlobalIds back as typed `LifecycleRow.of_task` rows graduating the geometry-minted `GeometrySubject` `"bim-lifecycle"` member through `graduation.md`'s `GeometryHandoff` carrier. A new schedule format is one `ScheduleFormat` row binding its `<Format>2Ifc` parser — never a new entry surface.
- evidence: each conversion captures the source format, the populated task count, and the schedule id; the lifecycle receipt keys the empty-row fraction (a non-empty source producing no tasks is a degenerate run) as the 4d residual the graduation leg folds against the caller ceiling.
- boundary: `ifc4d` owns 4D schedule conversion and task-tree authoring; a hand-rolled P6/MS Project XML parser is the deleted form; IFC parse stays `ifcopenshell` (the spine the `.file` slot threads), 5D cost stays `ifc5d`, model transformation stays `ifcpatch`, revision diff stays `ifcdiff`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifc4d`
- Owns: 4D construction-schedule round-trip between IFC `IfcWorkSchedule`/`IfcTask` and MS Project / Primavera P6 / Asta formats, and task-tree authoring
- Accept: a source schedule file plus a target `ifcopenshell.file`, feeding the `ifc/costing.md#LIFECYCLE` `SCHEDULE` phase
- Reject: a hand-rolled P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion; a parse-per-format function family where the `ScheduleFormat` row binds the named parser class

[CAPTURE_GAP]:
