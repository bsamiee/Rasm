# [INTERCHANGE]

Files from CAD and city tools enter through one importer per format.

## [01]-[IMPORTERS]

Table importers are extension operators, present in live and headless sessions and absent from factory runs:

| [INDEX] | [FORMAT]     | [CALL]                                                 | [BEHAVIOR]                                                       |
| :-----: | :----------- | :----------------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | STEP, IGES   | `import_scene.step(filepath=)`                         | `cascadio` through glTF, file units to meters, Z-up data upright |
|  [02]   | Rhino `.3dm` | `import_3dm.some_data(filepath=)`                      | Render meshes alone, a Brep without one imports empty            |
|  [03]   | DXF          | `import_scene.cad2cube_dxf(filepath=, recenter_mode=)` | `$INSUNITS` to meters, layers, blocks, hatches                   |
|  [04]   | IFC          | `bim.load_project(filepath=)`                          | Replaces the open file unless `should_start_fresh_session=False` |
|  [05]   | CityJSON     | `cityjson.import_file(filepath=, clean_scene=False)`   | `transform` scale applied, `clean_scene` default on              |

- `import_scene.step` reads a Z-up file upright at its default `up_axis="Y"`, `up_axis="Z"` swaps its Y and Z extents
- `import_3dm` reads layers as empties by default, `import_layers_as_empties=False` makes collections
- `import_3dm` reuses a collection only when its `rhid` ID property holds the layer's Rhino id, else it adds one with a numeric suffix
- `import_3dm` links top-level layers under its own `Layers` collection, a reused parent layer included a second time
- `cad2cube_dxf` moves the drawing's box center to the origin by default, `recenter_mode` `NONE` keeps its coordinates and `MIN_CORNER` its corner
- `cad2cube_dxf` holds no unit or scale property
- Importer add-on preferences (`step_importer`, `cad2cube`) seed the File > Import dialog alone, code calls take each operator's defaults
- Inch OBJ, STL, and PLY files import with `global_scale=0.0254` and millimeter ones with `0.001`
- `wm.obj_import` and `import_scene.fbx` default to forward -Z up Y, `wm.stl_import` and `wm.ply_import` to forward Y up Z with `use_scene_unit` off
- `wm.usd_export` takes `convert_scene_units` (`METERS`, `INCHES`, `FEET`)
- Operator presets sit under `SCRIPTS/presets/operator/<idname>`
- Rhino files cross through `.glb` for materials and `.3dm` for layers as collections, OBJ, FBX, and USD lose materials, units, or axes
- Objects from glTF and STEP imports hold `QUATERNION` rotation, and `matrix_world` holds their transform
- `rotation_euler`, `get_object_detail_summary`, and `get_object_info` read zero rotation on a `QUATERNION` object
- `obj.convert_rotation_mode(rotation_mode="XYZ")` turns a `QUATERNION` object to Euler with its keys, `matrix_world` unchanged
- `wm.spz_import(filepath=)` and splat `.ply` files import as point clouds with `type` `GAUSSIAN_SPLAT`
- `export_scene.gltf` writes point clouds unless `export_pointclouds=False`
- `bpy.types.FileHandler.label_with_extensions("IO_FH_<format>")` names a C file handler's menu label
- `wm.usd_import` covers `*.usd` alone in its `filter_glob`

## [02]-[MATERIALS]

Rhino and Blender share the metallic-roughness parameters through glTF:
- Materials from a Rhino glTF turn on `use_backface_culling`, read specular at half, and hold emission un-linearized
- `import_3dm` keeps base color, metallic, roughness, specular, IOR, transmission, emission, and alpha, and drops coat, sheen, and subsurface
- `import_3dm` loads embedded image files alone, leaves data images `sRGB`, connects no normal image, and drops a rhino3dm-written `Material`
- Layer materials of a `.3dm` fill every object's slot with its link forced to `OBJECT`
- Exports for Rhino take `export_scene.gltf(export_format="GLB", export_hierarchy_full_collections=True)`
- Rhino names imported objects after their meshes, an export renames each mesh after its object first
- Rhino reads a Blender glTF's default specular as 1.0, loses emission strength, sheen, and subsurface, and puts alpha into the base color

## [03]-[AUTHORING]

`rhino3dm` and `ezdxf` wheels of installed extensions write CAD files no Blender exporter covers:
- Rhino: `rhino3dm.File3dm` with one `Layer` and `ObjectAttributes.LayerIndex` on every object, objects on no layer fail `import_3dm` with `KeyError`
- DXF: `ezdxf.new("R2018")` with `doc.units = ezdxf.units.MM` setting `$INSUNITS`

## [04]-[BATCH]

`convert` runs in a headless process and answers `LiveSession` in the live one:

```python
# [HEADLESS_CALL] STEP files and inch OBJ files, each to glTF binary with collections as Rhino layers
from convert import convert
from results import as_result

options = {"wm.obj_import": {"global_scale": 0.0254}, "export_scene.gltf": {"export_hierarchy_full_collections": True}}
result = as_result(convert("<name>", "export_scene.gltf", ("<dir>/<part>.step", "<dir>/<model>.obj"), options=options))
```

- Importers resolve from `FileHandler.bl_file_extensions` and C importers' `filter_glob`, C importers first as File > Import lists them
- `importer` names the importer for suffixes its `filter_glob` covers and suffixes no importer covers, `.dxf` and `.usdc` need it
- `cityjson.import_file` raises in the batch's worldless scene, city models import through `headless.py run` on a file holding a world
- `options` maps an importer or exporter id to its keyword arguments
- `options` ids naming neither answer `UnknownOperator`, undeclared properties `UnknownOptions`
- Suffixes importers share with no lone C importer among them answer `AmbiguousImporter` without a named importer
- `Converted` holds the importer, the imported object names, their world extent in meters, and every file the exporter wrote
- Outputs go to `.artifacts/blender/<name>/<stem><ext>`, `convert.json` beside them holds one case per file in input order
- `Failed`, `NoImporter`, `AmbiguousImporter`, and `SharedStem` name files the batch skipped, every other file converts
