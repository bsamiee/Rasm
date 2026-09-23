# [INTERCHANGE]

Files from CAD and city tools enter through one importer per format, and every import ends with a world-extent check against a dimension the source states.

## [01]-[IMPORTERS]

Every importer below is an extension operator, present in the live session and in headless sessions and absent from factory runs:

| [INDEX] | [FORMAT]     | [CALL]                                                 | [BEHAVIOR]                                                       |
| :-----: | :----------- | :----------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | STEP         | `wm.occ_import_step(filepath=, files=[{"name": }])`    | OCP kernel, meters, Z-up data, no object rotation                |
|  [02]   | Rhino `.3dm` | `import_3dm.some_data(filepath=)`                      | Render meshes alone, a Brep without one imports empty            |
|  [03]   | DXF          | `import_scene.cad2cube_dxf(filepath=, recenter_mode=)` | `$INSUNITS` to meters, layers, blocks, hatches                   |
|  [04]   | IFC          | `bim.load_project(filepath=)`                          | Replaces the open file unless `should_start_fresh_session=False` |
|  [05]   | CityJSON     | `cityjson.import_file(filepath=, clean_scene=False)`   | `transform` scale applied, `clean_scene` default on              |

- Importers that iterate a `files` list import nothing from `filepath` alone, `wm.occ_import_step` raises `STEP file could not be opened`
- `import_3dm` reads layers as empties by default, `import_layers_as_empties=False` makes collections
- `cad2cube_dxf` moves the drawing's box center to the origin by default, `recenter_mode="NONE"` keeps its coordinates
- Imported glTF objects hold `QUATERNION` rotation mode, `rotation_euler` and both object tools then read zero rotation, `matrix_world` holds it

## [02]-[AUTHORING]

Blender's Python holds `OCP`, `rhino3dm`, and `ezdxf` beside `bpy`, factory runs included, a script writes CAD files directly for test inputs and for exports no Blender exporter covers:
- STEP: `OCP` shapes through `STEPControl_Writer` with `Interface_Static.SetCVal_s("write.step.unit", "MM")`
- Rhino: `rhino3dm.File3dm` with one `Layer` and `ObjectAttributes.LayerIndex` on every object, objects on no layer fail `import_3dm` with `KeyError`
- DXF: `ezdxf.new("R2018")` with `doc.units = ezdxf.units.MM` setting `$INSUNITS`

## [03]-[CHECK]

```python
# [HEADLESS_CALL] World extent of the objects an import added from their evaluated meshes, compared with a dimension the source states
import bpy
from mathutils import Vector

before = set(bpy.data.objects.keys())
bpy.ops.<importer>(<arguments>)
new = [o for o in bpy.data.objects if o.name not in before]
depsgraph = bpy.context.evaluated_depsgraph_get()
evaluated = [o.evaluated_get(depsgraph) for o in new if o.type in {"MESH", "CURVE"}]
points = [e.matrix_world @ Vector(v.co) for e in evaluated for v in e.to_mesh().vertices]
extent = [max(p[i] for p in points) - min(p[i] for p in points) for i in range(3)]
for e in evaluated:
    e.to_mesh_clear()
result = {"objects": [o.name for o in new], "extent_m": extent, "rotation_modes": {o.name: o.rotation_mode for o in new}}
```

- Extents off by 1000 or 25.4 mark a unit error and swapped axes an up-axis error, the import reruns with corrected arguments

## [04]-[BATCH]

`scripts/convert.py` imports each file into an empty scene of the session and writes it through one exporter:

```python
# [HEADLESS_CALL] STEP files through the OCP importer and OBJ files through the stock one, each to glTF binary, <root> is the repository root
import runpy

convert = runpy.run_path("<root>/.claude/skills/use-blender/scripts/convert.py")
result = convert["as_result"](convert["convert"]("<name>", "export_scene.gltf", ("<dir>/<part>.step", "<dir>/<part>.obj"), "wm.occ_import_step"))
```

- Importers resolve from each `FileHandler`'s `bl_file_extensions` and the `filter_glob` of C importers, a C importer before Python ones as File > Import lists them
- Fourth argument names the importer for the suffixes its `filter_glob` covers, `.dxf` and CityJSON have no file handler and need it
- Suffixes several Python importers read answer `AmbiguousImporter` without a named importer
- `Converted` extents read evaluated vertices, a file of empties, lights, or cameras alone answers `EmptyImport`
- Outputs land at `.artifacts/blender/<name>/<stem><ext>`, `convert.json` beside them holds one case per file in input order
- `Converted` carries imported objects and the evaluated world extent to compare with a dimension the source states
- `Failed`, `EmptyImport`, `NoImporter`, and `SharedStem` name files the batch skipped, every other file converts
