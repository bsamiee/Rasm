# [PY_GEOMETRY_API_IFCPATCH]

`ifcpatch` supplies the IFC model-transformation surface for the geometry ifc-analysis rail: a recipe registry applying named modification recipes to an `ifcopenshell.file` — schema migration, georeferencing offset, attribute/pset purge, spatial-tree reset, element extraction, and merge — through one `execute`/`write` entry over a `Recipe`-named arguments shape. It rides the `ifcopenshell` companion lane (`0.8.5`, depends `ifcopenshell`/`toposort`/`numpy`), so the analysis owner composes `ifcpatch.execute` directly rather than mutating the model through ad-hoc `file.add`/`remove`/`create_entity` loops.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcpatch`
- package: `ifcpatch`
- import: `import ifcpatch`
- owner: `geometry`
- rail: ifc-analysis / model-transformation
- installed: `0.8.5`, the IfcOpenShell-ecosystem companion-lane band (depends `ifcopenshell`, `toposort`, `numpy`)
- entry points: none (library only)
- capability: named-recipe IFC model transformation — schema migration, coordinate/georeference offset, attribute and pset purge, spatial-decomposition reset, sub-model extraction by selector, model merge, and IFC-to-CSV/SQL conversion — applied through one recipe-dispatched execute over an `ifcopenshell.file`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: patch arguments and recipe registry
- rail: model-transformation

| [INDEX] | [SYMBOL]                 | [PACKAGE_ROLE]   | [CAPABILITY]                                               |
| :-----: | :----------------------- | :--------------- | :--------------------------------------------------------- |
|  [01]   | `ifcpatch.Patcher`       | recipe base      | the per-recipe transformation contract over `(file, args)` |
|  [02]   | `recipes.*`              | recipe namespace | the named recipe modules (one transformation each)         |
|  [03]   | `ExtractElements`        | recipe           | extract a sub-model by selector query into a new file      |
|  [04]   | `Migrate`                | recipe           | migrate a model to a target IFC schema version             |
|  [05]   | `OffsetObjectPlacements` | recipe           | apply a coordinate offset to every object placement        |
|  [06]   | `MergeProjects`          | recipe           | merge two IFC projects into one model                      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe execution and output
- rail: model-transformation

The execute entry dispatches a recipe by name over an `ifcopenshell.file` and a recipe-specific arguments list; the output entry serializes the patched model or the recipe's non-IFC product.

| [INDEX] | [SURFACE]                                                 | [CALL_SHAPE]               | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------- | :------------------------- | :--------------------------------------------- |
|  [01]   | `ifcpatch.execute({"input","file","recipe","arguments"})` | recipe name plus args dict | run a named recipe, return the patched output  |
|  [02]   | `ifcpatch.write(output, filepath)`                        | recipe output plus path    | serialize the patched model or product         |
|  [03]   | `ifcpatch.extract_docs(recipe, ...)`                      | recipe name                | extract a recipe's docstring and argument spec |

## [04]-[IMPLEMENTATION_LAW]

[PATCH_TOPOLOGY]:
- import: `import ifcpatch` at boundary scope only; module-level import is banned by the manifest import policy.
- recipe axis: `ifcpatch.execute` dispatches on the `recipe` name into the `recipes` namespace, each recipe a `Patcher` subclass over `(file, arguments)`; the recipe is a closed name vocabulary, never a per-transformation execute function family. Adding a transformation is one recipe name, never a new entry surface.
- output axis: `ifcpatch.write` serializes either the patched `ifcopenshell.file` or a recipe's non-IFC product (a CSV/SQL conversion recipe returns a string/path), so the output type is recipe-determined and the write is one polymorphic sink.
- evidence: each patch captures the recipe name, the input identity, and the patched-model identity as a transformation receipt.
- boundary: `ifcpatch` owns named IFC model transformation; ad-hoc `file.create_entity`/`add`/`remove` mutation loops are the deleted form where a recipe owns the transformation; IFC parse stays `ifcopenshell`, model diff stays `ifcdiff`, 5D cost stays `ifc5d`.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcpatch`
- Owns: named-recipe IFC model transformation (migration, offset, purge, extract, merge, convert) over an `ifcopenshell.file`
- Accept: an `ifcopenshell.file` plus a recipe name and recipe-specific arguments, feeding the ifc-analysis transformation owner
- Reject: a hand-rolled mutation loop where a recipe owns the transformation; a per-recipe execute function family over the `recipe` name row

[CAPTURE_GAP]:
- floor: companion interpreter cp313; `ifcpatch==0.8.5` rides the `ifcopenshell` companion lane, so reflection resolves where `ifcopenshell` resolves; the `>=3.15` project venv carries no `ifcopenshell` core
- members: the `ifcpatch.execute` arguments-dict key shape (`input`/`file`/`recipe`/`arguments`), the `ifcpatch.write` output-type dispatch, and the `recipes` namespace recipe names confirm by introspection against the installed companion distribution before any fence transcribes them
