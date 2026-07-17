# [PY_GEOMETRY_API_IFCPATCH]

`ifcpatch` supplies the IFC model-transformation surface for the geometry ifc-analysis rail: a recipe registry applying named modification recipes to an `ifcopenshell.file` — schema migration, georeference/world-coordinate offset, attribute/pset purge, spatial-tree reset, element extraction by selector, project merge, unit conversion, and IFC-to-SQL/CSV conversion — through one `execute(args: ArgumentsDict)`/`write(output, filepath)` entry over a `recipe`-named `ArgumentsDict`. Each recipe is a `BasePatcher` subclass with a `patch()`/`get_output()` contract. It rides the `ifcopenshell` worker lane (`0.8.5`; depends `ifcopenshell`, `toposort`, `numpy`), so the lifecycle owner composes `ifcpatch.execute(...)` directly rather than mutating the model through ad-hoc `file.add`/`remove`/`create_entity` loops.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcpatch`
- package: `ifcpatch`
- import: `import ifcpatch`
- owner: `geometry`
- rail: ifc-analysis / model-transformation
- installed: `0.8.5`
- entry points: none (library only)
- capability: named-recipe IFC model transformation — schema migration, coordinate/georeference/world-coordinate offset, attribute and pset purge, spatial-decomposition reset, sub-model extraction by selector query, project merge, unit conversion, GlobalId regeneration, mesh tessellation, and IFC-to-SQLite/CSV conversion — applied through one recipe-dispatched `execute` over an `ArgumentsDict` whose `recipe` key routes into the `recipes` namespace, each recipe a `BasePatcher` subclass; the output is recipe-determined (a patched `ifcopenshell.file` or a non-IFC string/path) and serialized through one polymorphic `write`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: patch arguments, recipe base, and doc metadata
- rail: model-transformation

`ArgumentsDict` `TypedDict` carries `recipe: str` (required) plus `file: ifcopenshell.file`/`input: str`/`log: str`/`arguments: Sequence[Any]` (all `NotRequired`); `BasePatcher` runs `__init__(file, logger)` -> `patch() -> None` -> `get_output() -> ifcopenshell.file | str | None`.

| [INDEX] | [SYMBOL]        | [PACKAGE_ROLE]   | [CAPABILITY]                                                          |
| :-----: | :-------------- | :--------------- | :-------------------------------------------------------------------- |
|  [01]   | `ArgumentsDict` | execute payload  | the `execute` payload; `recipe` routes into `recipes` (shape in lead) |
|  [02]   | `BasePatcher`   | recipe base      | per-recipe transformer, one subclass each (contract in lead)          |
|  [03]   | `recipes.*`     | recipe namespace | the named recipe modules, one `BasePatcher` subclass each             |
|  [04]   | `PatcherDoc`    | doc carrier      | `TypedDict` recipe metadata (name/description/inputs)                 |
|  [05]   | `InputDoc`      | arg doc carrier  | `TypedDict` per-argument metadata (name/type/description/default)     |
|  [06]   | `DocstringData` | parse result     | structured `parse_docstring` output (summary plus argument table)     |

[RECIPE_NAMESPACE]: the `recipes` module names (each a `recipe` key value) include `ExtractElements`, `Migrate`, `OffsetObjectPlacements`, `OffsetStoreyElevations`, `ResetAbsoluteCoordinates`, `ResetSpatialElementLocations`, `SetWorldCoordinateSystem`, `SetFalseOrigin`, `SetRefElevation`, `MergeProjects`, `MergeDuplicateTypes`, `MergeStyles`, `PurgeData`, `UnsharePsets`, `ConvertLengthUnit`, `ConvertNestToAggregate`, `ConvertPropertiesToQuantities`, `Ifc2Sql`, `ExtractPropertiesToSQLite`, `RegenerateGlobalIds`, `TessellateElements`, `Optimise`, `RecycleNonRootedElements`, `SplitByBuildingStorey`, `DowngradeIndexedPolyCurve`, `AssignConstituentFractions`, `RemoveSiteRepresentation`, `AGS2IFC`, plus the Revit/ArchiCAD interop-fix recipes (`FixRevitTINs`, `FixRevit2025TINs`, `FixRevitClassificationCodeTypes`, `RemoveRevitUniformatClassification`, `FixArchiCADToRevitDoorSwings`, `FixArchiCADToRevitSpaces`) and the alignment recipes (`AddGeometricRepresentationToAlignment`, `AddZeroLengthSegmentToAlignment`, `AddLinearPlacementFallbackPosition`, `PatchStationReferentPosition`).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe execution, output, and doc introspection
- rail: model-transformation

`execute` dispatches a recipe by name over the `ArgumentsDict`; `write` serializes the recipe-determined output; `extract_docs(submodule_name, cls_name, method_name="__init__", boilerplate_args=None)` introspects a recipe's argument contract.

| [INDEX] | [SURFACE]                                                                    | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `ifcpatch.execute(args: ArgumentsDict) -> ifcopenshell.file \| str \| None`  | run the `recipe`-named transformation             |
|  [02]   | `ifcpatch.write(output, filepath: Path \| str) -> None`                      | serialize the patched model or non-IFC product    |
|  [03]   | `ifcpatch.extract_docs(submodule_name, cls_name, ...) -> PatcherDoc \| None` | introspect a recipe's argument contract           |
|  [04]   | `ifcpatch.parse_docstring(docstring: str) -> DocstringData`                  | parse a recipe docstring into structured metadata |
|  [05]   | `ifcpatch.ensure_logger(logger=None) -> logging.Logger`                      | resolve the logger the `BasePatcher` records into |

`execute` takes ONE positional `ArgumentsDict`, not a keyword call — the consumer builds `{"recipe": name, "file": model, "arguments": [...], "input": ""}` and passes it whole; `recipe` is the only required key. `extract_docs` is the recipe-introspection entry over `(submodule_name, cls_name)`, NOT a single dotted recipe name.

## [04]-[IMPLEMENTATION_LAW]

[PATCH_TOPOLOGY]:
- import: `import ifcpatch` at boundary scope only; module-level import is banned by the manifest import policy.
- recipe axis: `ifcpatch.execute(args)` reads `args["recipe"]`, imports `ifcpatch.recipes.<recipe>`, constructs its `BasePatcher` subclass with `args["file"]` plus the resolved logger, runs `patch()`, and returns `get_output()`; the recipe is a closed name vocabulary over the `recipes` namespace, never a per-transformation execute function family. Adding a transformation is one recipe module, never a new entry surface.
- output axis: `ifcpatch.write(output, filepath)` serializes either a patched `ifcopenshell.file` (writes the model) or a recipe's non-IFC product (a `str`/path a SQL/CSV recipe like `Ifc2Sql`/`ExtractPropertiesToSQLite` returns), so the output type is recipe-determined and the write is one polymorphic sink discriminated on `isinstance(output, ifcopenshell.file)`.
- logger axis: `BasePatcher.__init__(file, logger)` records into the resolved logger; `ensure_logger(None)` mints a default, so a recipe's diagnostic stream is one logger contract, not per-recipe ad-hoc printing.
- evidence: each patch captures the recipe name, the input identity, and the patched-model schema or the non-IFC product type as a transformation receipt.
- boundary: `ifcpatch` owns named IFC model transformation; ad-hoc `file.create_entity`/`add`/`remove` mutation loops are the deleted form where a recipe owns the transformation; the columnar SQL/CSV product write defers to the data boundary, never a throwaway run sink; IFC parse stays `ifcopenshell`, model diff stays `ifcdiff`, 5D cost stays `ifc5d`.

[INTEGRATION_STACK]:
- `geometry:ifc/costing.md#LIFECYCLE` `PATCH` phase composes `ifcpatch.execute({"input": "", "file": model, "recipe": recipe, "arguments": decode(args, type=list[object])})` then `ifcpatch.write(output, path)`, discriminating the output via `isinstance(output, ifcopenshell.file)` to project the patched `.schema` or the non-IFC product type as the `LifecycleRow.of_patch` row — the recipe name is the closed `recipe` key, the `arguments` a parameterized `list[object]`, never an untyped list.
- `ExtractElements` recipe STACKS with `geometry:ifc/selector.md#SELECTOR`: its `arguments[0]` is a selector-grammar query, so a sub-model extraction threads the validated `SelectorQuery.filter_string` rather than a raw query, the same gate the analysis and diff owners share.
- `Ifc2Sql`/`ExtractPropertiesToSQLite` recipes STACK with the data tier: their non-IFC string/path output is the columnar product `python:data/spatial` owns, so the lifecycle owner reads the product type as a typed row and defers the durable write, never constructing a throwaway `tempfile` the run discards.

## [05]-[LOCAL_ADMISSION]

[RAIL_LAW]:
- Package: `ifcpatch`
- Owns: named-recipe IFC model transformation (migration, offset, purge, extract, merge, convert, regenerate, tessellate) over an `ifcopenshell.file` through `execute`/`write`, each recipe a `BasePatcher` subclass
- Accept: an `ArgumentsDict` with a `recipe` name plus the target `file` and recipe-specific `arguments`, feeding the ifc-analysis transformation owner
- Reject: a hand-rolled mutation loop where a recipe owns the transformation; a per-recipe execute function family over the `recipe` name; a `Patcher` base name (the real base is `BasePatcher`); a single-dotted `extract_docs(recipe)` call (the real arity is `(submodule_name, cls_name)`); a throwaway temp-file sink where the non-IFC product defers to the data boundary

[CAPTURE_GAP]:
- members: the `execute(args: ArgumentsDict)` single-arg dispatch, the `ArgumentsDict` key shape (`recipe` required; `file`/`input`/`log`/`arguments` NotRequired), the `write(output, filepath)` polymorphic sink, the `BasePatcher` `__init__(file, logger)`/`patch`/`get_output` contract, the `extract_docs(submodule_name, cls_name, method_name="__init__", boilerplate_args=None)` introspection arity, and the `recipes` namespace recipe names are source-confirmed; the exact per-recipe `arguments` element order is a per-recipe `extract_docs`/source detail the consumer reads from the recipe `__init__` signature
