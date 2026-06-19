# [PY_GEOMETRY_API_IFC4D]

`ifc4d` supplies the IFC 4D construction-scheduling surface for the geometry ifc-analysis rail: round-trip conversion between IFC `IfcWorkSchedule`/`IfcTask` task trees and the external scheduling formats (Microsoft Project XML, Oracle Primavera P6 XML/XER, Asta Powerproject), plus task-tree authoring and duration/sequence calculation over the `ifcopenshell` model. It rides the `ifcopenshell` companion lane (`0.8.5`, depends `ifcopenshell`/`typing-extensions`), so the analysis owner composes `ifc4d` parsers and writers directly rather than re-deriving a schedule-task graph.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifc4d`
- package: `ifc4d`
- import: `import ifc4d`
- owner: `geometry`
- rail: ifc-analysis / 4d-scheduling
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`, `typing-extensions`)
- entry points: none (library only)
- capability: parse Microsoft Project XML / Primavera P6 XML/XER / Asta Powerproject into an `IfcWorkSchedule`/`IfcTask` task tree, write an IFC schedule back to MS Project / P6 XML, and author/sequence construction tasks over the `ifcopenshell` model

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: schedule parsers and writers
- rail: 4d-scheduling

| [INDEX] | [SYMBOL]             | [PACKAGE_ROLE] | [CAPABILITY]                                    |
| :-----: | :------------------- | :------------- | :---------------------------------------------- |
|   [1]   | `MSProject2Ifc`      | parser         | Microsoft Project XML into an IFC work schedule |
|   [2]   | `P62Ifc`             | parser         | Primavera P6 XML/XER into an IFC work schedule  |
|   [3]   | `Asta2Ifc`           | parser         | Asta Powerproject into an IFC work schedule     |
|   [4]   | `Ifc2P6` / `Ifc2MSP` | writer         | IFC work schedule back to P6 XML / MS Project   |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: schedule conversion
- rail: 4d-scheduling

Parser rows consume a source schedule file plus a target `ifcopenshell.file`; writer rows consume an `ifcopenshell.file` plus a schedule entity.

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]                  | [CAPABILITY]                                 |
| :-----: | :-------------------------- | :---------------------------- | :------------------------------------------- |
|   [1]   | `MSProject2Ifc().execute()` | source path plus target model | parse MS Project XML, write the IFC schedule |
|   [2]   | `P62Ifc().execute()`        | source path plus target model | parse P6 XML/XER, write the IFC schedule     |
|   [3]   | `Ifc2P6().execute()`        | model plus schedule plus path | export the IFC schedule to P6 XML            |

## [4]-[IMPLEMENTATION_LAW]

[FOUR_D_TOPOLOGY]:
- import: `import ifc4d.msproject2ifc` / `import ifc4d.p62ifc` at boundary scope only; module-level import is banned by the manifest import policy.
- conversion axis: each parser is a `<Format>2Ifc` class with `.file`/`.xml`/`.work_plan` slots and an `execute()` that populates `IfcWorkSchedule`/`IfcTask`/`IfcRelSequence` into the target model; the format is the named parser class, never a parse-per-format function family on one entry — the scheduling-format vocabulary is the closed parser set.
- writer axis: `Ifc2<Format>` is the symmetric export over the IFC schedule entity.
- evidence: each conversion captures the source format, the task count, and the schedule id as a 4d receipt.
- boundary: `ifc4d` owns 4D schedule conversion and task-tree authoring; a hand-rolled P6/MS Project XML parser is the deleted form; IFC parse stays `ifcopenshell`, 5D cost stays `ifc5d`, model transformation stays `ifcpatch`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifc4d`
- Owns: 4D construction-schedule round-trip between IFC `IfcWorkSchedule`/`IfcTask` and MS Project / Primavera P6 / Asta formats, and task-tree authoring
- Accept: a source schedule file plus a target `ifcopenshell.file`, feeding the ifc-analysis 4D owner
- Reject: a hand-rolled P6/MS Project XML parser where the `<Format>2Ifc` parser owns the conversion

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifc4d==0.8.5` rides the `ifcopenshell` companion lane, so reflection resolves where `ifcopenshell` resolves; the `>=3.15` project venv carries no `ifcopenshell` core
- members: the `<Format>2Ifc`/`Ifc2<Format>` parser/writer class names, the `.execute()` entry, and the populated-entity set (`IfcWorkSchedule`/`IfcTask`/`IfcRelSequence`) confirm by introspection against the installed companion distribution before any fence transcribes them
