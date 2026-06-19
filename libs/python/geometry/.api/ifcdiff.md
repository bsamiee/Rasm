# [PY_GEOMETRY_API_IFCDIFF]

`ifcdiff` supplies the IFC model-comparison surface for the geometry ifc-analysis rail: a structured diff between two `ifcopenshell.file` models — added/deleted/changed elements across geometry, properties, and structure — through the `IfcDiff` owner over `deepdiff`, emitting a per-element change record the analysis owner graduates as as-designed-versus-revision evidence. It rides the `ifcopenshell` companion lane (`0.8.5`, depends `ifcopenshell`/`deepdiff`).

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcdiff`
- package: `ifcdiff`
- import: `import ifcdiff`
- owner: `geometry`
- rail: ifc-analysis / model-diff
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`, `deepdiff`)
- entry points: none (library only)
- capability: structured comparison of two IFC models — added, deleted, and changed elements keyed by GlobalId, with geometry/property/structure change classification through `deepdiff`, and a JSON change-report export

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: diff owner
- rail: model-diff

| [INDEX] | [SYMBOL]                  | [PACKAGE_ROLE] | [CAPABILITY]                                             |
| :-----: | :------------------------ | :------------- | :------------------------------------------------------- |
|   [1]   | `IfcDiff`                 | diff root      | drives the two-model comparison, holds the change result |
|   [2]   | `IfcDiff.change_register` | change map     | GlobalId-keyed added/deleted/changed element record      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: diff execution and export
- rail: model-diff

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]            | [CAPABILITY]                             |
| :-----: | :--------------------------------- | :---------------------- | :--------------------------------------- |
|   [1]   | `IfcDiff(old_file, new_file, ...)` | two `ifcopenshell.file` | construct the comparison over two models |
|   [2]   | `IfcDiff.diff()`                   | none                    | run the structured comparison            |
|   [3]   | `IfcDiff.export(path)`             | path                    | write the change register to JSON        |
|   [4]   | `IfcDiff.change_register`          | none                    | the GlobalId-keyed change map post-diff  |

## [4]-[IMPLEMENTATION_LAW]

[DIFF_TOPOLOGY]:
- import: `import ifcdiff` at boundary scope only; module-level import is banned by the manifest import policy.
- diff axis: `IfcDiff(old, new).diff()` populates `change_register` with the added/deleted/changed element classification keyed by GlobalId; the change classification (geometry, attribute, pset, relationship) rides `deepdiff` over the per-element attribute graph, never a hand-rolled attribute walk.
- evidence: each diff captures the old/new model identities, the added/deleted/changed counts, and the per-element change kind as a diff receipt; the change register graduates as revision-comparison evidence.
- boundary: `ifcdiff` owns IFC model comparison; a hand-rolled `by_type`/attribute-walk diff is the deleted form; model transformation stays `ifcpatch`, 5D cost stays `ifc5d`, IFC parse stays `ifcopenshell`.

## [5]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcdiff`
- Owns: structured two-model IFC comparison (added/deleted/changed by GlobalId across geometry/property/structure) and JSON change-report export
- Accept: two `ifcopenshell.file` models, feeding the ifc-analysis diff owner
- Reject: a hand-rolled `by_type` attribute-walk diff where `IfcDiff` and `deepdiff` own the comparison

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifcdiff==0.8.5` rides the `ifcopenshell` companion lane, so reflection resolves where `ifcopenshell` resolves; the `>=3.15` project venv carries no `ifcopenshell` core
- members: the `IfcDiff(old, new)` constructor arity, the `diff()`/`export(path)` entries, and the `change_register` map shape confirm by introspection against the installed companion distribution before any fence transcribes them
