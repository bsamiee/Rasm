# [INTERCHANGE]

Files from CAD and city tools enter through one importer per format, and every import ends with a world-extent check against a dimension the source states.

## [01]-[IMPORTERS]

Every importer below is an extension operator, present in the live session and in headless sessions and absent from factory runs:

| [INDEX] | [FORMAT]     | [CALL]                                                 | [BEHAVIOR]                                                       |
| :-----: | :----------- | :----------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | STEP, IGES   | `import_scene.step(filepath=)`                         | OCP through glTF, file units to meters, Z-up data upright        |
|  [02]   | Rhino `.3dm` | `import_3dm.some_data(filepath=)`                      | Render meshes alone, a Brep without one imports empty            |
|  [03]   | DXF          | `import_scene.cad2cube_dxf(filepath=, recenter_mode=)` | `$INSUNITS` to meters, layers, blocks, hatches                   |
|  [04]   | IFC          | `bim.load_project(filepath=)`                          | Replaces the open file unless `should_start_fresh_session=False` |
|  [05]   | CityJSON     | `cityjson.import_file(filepath=, clean_scene=False)`   | `transform` scale applied, `clean_scene` default on              |

- `import_scene.step` reads a Z-up file upright at its default `up_axis="Y"`, `up_axis="Z"` swaps its Y and Z extents
- `import_3dm` reads layers as empties by default, `import_layers_as_empties=False` makes collections
- `cad2cube_dxf` moves the drawing's box center to the origin by default, `recenter_mode="NONE"` keeps its coordinates
- OBJ, STL, and PLY hold no unit, an inch file imports with `global_scale=0.0254` and a millimeter file with `0.001`
- `wm.obj_import` and `import_scene.fbx` default to forward -Z up Y, `wm.stl_import` and `wm.ply_import` to forward Y up Z with `use_scene_unit` off
- `wm.usd_export` takes `convert_scene_units` (`METERS`, `INCHES`, `FEET`), operator presets sit under `SCRIPTS/presets/operator/<idname>`
- Rhino files cross through `.glb` for materials and `.3dm` for layers as collections, OBJ, FBX, and USD lose materials, units, or axes
- Objects from glTF and STEP imports hold `QUATERNION` rotation, `rotation_euler` and both object tools read zero, `matrix_world` holds it

## [02]-[MATERIALS]

Rhino and Blender share the metallic-roughness parameters through glTF, each import's materials read back against the source's values:
- Materials from a Rhino glTF turn on `use_backface_culling`, read specular at half, and hold emission un-linearized
- `import_3dm` carries base color, metallic, roughness, specular, IOR, transmission, emission, and alpha, and drops coat, sheen, and subsurface
- `import_3dm` loads only files embedded in the `.3dm`, leaves data images `sRGB`, and connects no normal image
- Layer materials of a `.3dm` fill every object's slot with its link forced to `OBJECT`
- Exports for Rhino take `export_scene.gltf(export_format="GLB", export_hierarchy_full_collections=True)`
- Rhino names imported objects after their meshes, an export renames each mesh after its object first
- Rhino reads a Blender glTF's default specular as 1.0, loses emission strength, sheen, and subsurface, and puts alpha into the base color
- Object, Generated, and Box projections stay inside Blender, an export carries UVs and the Mapping node scale alone

## [03]-[AUTHORING]

Blender's Python holds `OCP`, `rhino3dm`, and `ezdxf` beside `bpy`, factory runs included, a script writes CAD files directly for test inputs and for exports no Blender exporter covers:
- STEP: `OCP` shapes through `STEPControl_Writer` with `Interface_Static.SetCVal_s("write.step.unit", "MM")`
- Rhino: `rhino3dm.File3dm` with one `Layer` and `ObjectAttributes.LayerIndex` on every object, objects on no layer fail `import_3dm` with `KeyError`
- DXF: `ezdxf.new("R2018")` with `doc.units = ezdxf.units.MM` setting `$INSUNITS`

## [04]-[CHECK]

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

## [05]-[BATCH]

`scripts/convert.py` imports each file into an empty factory scene of the session and writes it through one exporter:

```python
# [HEADLESS_CALL] STEP files and inch OBJ files, each to glTF binary with collections as Rhino layers, <skill> is this skill's directory
import runpy

convert = runpy.run_path("<skill>/scripts/convert.py")
options = {"wm.obj_import": {"global_scale": 0.0254}, "export_scene.gltf": {"export_hierarchy_full_collections": True}}
result = convert["as_result"](convert["convert"]("<name>", "export_scene.gltf", ("<dir>/<part>.step", "<dir>/<part>.obj"), options=options))
```

- Importers resolve from `FileHandler.bl_file_extensions` and C importers' `filter_glob`, C importers first as File > Import lists them
- Fourth argument names the importer for suffixes its `filter_glob` covers and suffixes no importer covers, `.dxf`, CityJSON, and `.usdc` need it
- `options` maps an importer or exporter id to its keyword arguments, a property the operator lacks answers `UnknownOptions`
- Suffixes more than one Python importer reads answer `AmbiguousImporter` without a named importer
- `Converted` carries imported objects and their evaluated world extent, a file of empties, lights, or cameras alone answers `EmptyImport`
- Outputs land at `.artifacts/blender/<name>/<stem><ext>`, `convert.json` beside them holds one case per file in input order
- `Failed`, `EmptyImport`, `NoImporter`, and `SharedStem` name files the batch skipped, every other file converts
