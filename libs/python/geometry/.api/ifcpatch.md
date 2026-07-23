# [PY_GEOMETRY_API_IFCPATCH]

`ifcpatch` owns named-recipe IFC model transformation over the `ifcopenshell` model: `execute(args)` routes an `ArgumentsDict` by its `recipe` key into the `recipes` namespace where each recipe is a `BasePatcher` subclass running `patch()` -> `get_output()`, and one polymorphic `write` serializes the recipe-determined output — a patched `ifcopenshell.file` or a non-IFC `str`/path. It feeds the geometry ifc-analysis transformation rail, so the lifecycle owner composes `ifcpatch.execute(...)`/`write(...)` rather than an ad-hoc `file.create_entity`/`add`/`remove` mutation loop.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ifcpatch`
- package: `ifcpatch` (LGPL-3.0-or-later)
- import: `import ifcpatch`
- owner: `geometry`
- rail: ifc-analysis / model-transformation
- entry points: none (library only)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: patch arguments, recipe base, and doc-introspection metadata

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                                    |
| :-----: | :-------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `ArgumentsDict` | typed-dict    | `execute` payload; `recipe` required, `file`/`input`/`log`/`arguments` optional |
|  [02]   | `BasePatcher`   | class         | per-recipe transformer base; `__init__(file, logger)`/`patch`/`get_output`      |
|  [03]   | `PatcherDoc`    | typed-dict    | recipe metadata — name, description, `inputs` map                               |
|  [04]   | `InputDoc`      | typed-dict    | per-argument metadata — name, type, description, default                        |
|  [05]   | `DocstringData` | typed-dict    | structured `parse_docstring` output                                             |

[RECIPES]: `ExtractElements` `Migrate` `OffsetObjectPlacements` `OffsetStoreyElevations` `ResetAbsoluteCoordinates` `ResetSpatialElementLocations` `SetWorldCoordinateSystem` `SetFalseOrigin` `SetRefElevation` `MergeProjects` `MergeDuplicateTypes` `MergeStyles` `PurgeData` `UnsharePsets` `ConvertLengthUnit` `ConvertNestToAggregate` `ConvertPropertiesToQuantities` `Ifc2Sql` `ExtractPropertiesToSQLite` `RegenerateGlobalIds` `TessellateElements` `Optimise` `RecycleNonRootedElements` `SplitByBuildingStorey` `DowngradeIndexedPolyCurve` `AssignConstituentFractions` `RemoveSiteRepresentation` `AGS2IFC`

[RECIPES_INTEROP_FIX]: `FixRevitTINs` `FixRevit2025TINs` `FixRevitClassificationCodeTypes` `RemoveRevitUniformatClassification` `FixArchiCADToRevitDoorSwings` `FixArchiCADToRevitSpaces`

[RECIPES_ALIGNMENT]: `AddGeometricRepresentationToAlignment` `AddZeroLengthSegmentToAlignment` `AddLinearPlacementFallbackPosition` `PatchStationReferentPosition`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: recipe execution, output serialization, and doc introspection

| [INDEX] | [SURFACE]                                                           | [SHAPE] | [CAPABILITY]                                      |
| :-----: | :------------------------------------------------------------------ | :------ | :------------------------------------------------ |
|  [01]   | `execute(args) -> ifcopenshell.file \| str \| None`                 | static  | run the `recipe`-named transformation             |
|  [02]   | `write(output, filepath) -> None`                                   | static  | serialize the patched model or non-IFC product    |
|  [03]   | `extract_docs(submodule_name, cls_name, ...) -> PatcherDoc \| None` | static  | introspect a recipe's argument contract           |
|  [04]   | `parse_docstring(docstring) -> DocstringData`                       | static  | parse a recipe docstring into structured metadata |
|  [05]   | `ensure_logger(logger) -> logging.Logger`                           | static  | resolve the `BasePatcher` logging target          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- recipe axis: `execute(args)` reads `args["recipe"]`, imports `ifcpatch.recipes.<recipe>`, constructs its `BasePatcher` subclass with `args["file"]` and the resolved logger, runs `patch()`, and returns `get_output()`; the recipe is a closed name vocabulary over `recipes`, never a per-transformation execute-function family, so a new transformation is one recipe module.
- output axis: `write(output, filepath)` is one polymorphic sink discriminating on `isinstance(output, ifcopenshell.file)` — it writes a patched model or a recipe's non-IFC `str`/path product (`Ifc2Sql`, `ExtractPropertiesToSQLite`), so the output type is recipe-determined.
- logger axis: `BasePatcher.__init__(file, logger)` records into the resolved logger and `ensure_logger(None)` mints the `IFCPatch` default, so recipe diagnostics ride one logger contract rather than per-recipe printing.

[STACKING]:
- `ifcopenshell`(`.api/ifcopenshell.md`): `execute` mutates an `ifcopenshell.file` in place through the recipe's internal `create_entity`/`add`/`remove` calls and `write` serializes it via the SPF writer, so the model handle is the one the parse owner mints.
- `ifc/costing.md`(`#LIFECYCLE`): its `PATCH` phase composes `execute({"recipe": name, "file": model, "arguments": decode(args, type=list[object]), "input": ""})` then `write(output, path)`, discriminating `isinstance(output, ifcopenshell.file)` to project the patched `.schema` or the non-IFC product type as the `LifecycleRow.of_patch` row.
- `ifc/selector.md`(`#SELECTOR`): `ExtractElements` reads `arguments[0]` as a selector-grammar query, threading the validated `SelectorQuery.filter_string`, the same gate the analysis and diff owners share.
- `python:data/spatial`: `Ifc2Sql`/`ExtractPropertiesToSQLite` emit a non-IFC `str`/path product the data tier owns, so the lifecycle owner reads the product type as a typed row and defers the durable write.

[LOCAL_ADMISSION]:
- Geometry's ifc-analysis transformation owner composes `ifcpatch.execute`/`write` directly; model modification never re-derives against an ad-hoc `create_entity`/`add`/`remove` loop, and the columnar SQL/CSV product write defers to the data boundary.

[RAIL_LAW]:
- Package: `ifcpatch`
- Owns: named-recipe IFC model transformation over an `ifcopenshell.file` — one `execute` dispatch across the `recipes` namespace and one polymorphic `write`, each recipe a `BasePatcher` subclass
- Accept: an `ArgumentsDict` carrying a `recipe` name, the target `file`, and recipe-specific `arguments`, feeding the ifc-analysis transformation owner
- Reject: a hand-rolled `create_entity`/`add`/`remove` mutation loop where a recipe owns the transformation; a per-recipe execute-function family over the `recipe` name; a single-dotted `extract_docs(recipe)` call where the arity is `(submodule_name, cls_name)`; a throwaway temp-file sink where the non-IFC product defers to the data boundary
