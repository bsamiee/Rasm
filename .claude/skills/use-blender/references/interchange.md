# [INTERCHANGE]

CAD, mesh, and city files enter through one importer per format, and `rhino3dm` and `ezdxf` write the CAD files Blender has no exporter for.

## [01]-[IMPORT]

One file enters the user's scene live, or a session through `call` on a file `headless.py start` opened, and the objects it added report their collections and size:

```python
# [EXECUTE_BLENDER_CODE] <file> imported, with the collections and world extent in meters of each object it added
import bpy
from scene import bounds

before = set(bpy.data.objects)
status = bpy.ops.<importer>(filepath="<file>", <arguments>)
added = set(bpy.data.objects) - before
extents = bounds(bpy.context.evaluated_depsgraph_get(), drawn=False)
result = {
    "status": sorted(status),
    "collections": sorted({collection.name for obj in added for collection in obj.users_collection}),
    "extents": {obj.name: (extents[obj.name][1] - extents[obj.name][0]).round(4).tolist() for obj in added if obj.name in extents},
}
```

Each format takes its importer with the arguments that put its units and axes right:

| [INDEX] | [FORMAT]     | [IMPORTER]                       | [ARGUMENTS]                      | [UNITS]                                         |
| :-----: | :----------- | :------------------------------- | :------------------------------- | :---------------------------------------------- |
|  [01]   | STEP, IGES   | `import_scene.step`              | None                             | File unit to meters, `up_axis` `Y`              |
|  [02]   | Rhino `.3dm` | `import_3dm.some_data`           | `import_layers_as_empties=False` | Model unit to meters, layers as collections     |
|  [03]   | DXF          | `import_scene.cad2cube_dxf`      | `recenter_mode="NONE"`           | `$INSUNITS` to meters, coordinates kept         |
|  [04]   | CityJSON     | `cityjson.import_file`           | `clean_scene=False`              | Transform applied, minimum vertex at the origin |
|  [05]   | OBJ          | `wm.obj_import`                  | `global_scale`                   | No unit, forward -Z, up Y                       |
|  [06]   | STL, PLY     | `wm.stl_import`, `wm.ply_import` | `global_scale`                   | No unit, forward Y, up Z                        |
|  [07]   | GLB, glTF    | `import_scene.gltf`              | None                             | Meters, Y-up converted to Z-up                  |
|  [08]   | USD          | `wm.usd_import`                  | None                             | `metersPerUnit` applied, `.usda`, `.usdc` read  |

- `global_scale` takes `0.0254` for an inch file and `0.001` for a millimeter file
- Importer preferences (`step_importer`, `cad2cube`) seed the File > Import dialog alone, a call from code takes the operator defaults
- `cad2cube_dxf` at its default `recenter_mode` `BBOX` moves the drawing's box center to the origin
- `cityjson.import_file` writes its origin offset on `scene.world` and raises in a scene without one, later files into that world share the offset
- `import_3dm` meshes Breps from their render meshes, a Brep saved without one imports as a mesh with no vertices
- Extents off by 1000 or 25.4 from a dimension the source states mark a unit error, swapped extents an axis error
- Objects from glTF and STEP hold `QUATERNION` rotation, `rotation_euler` reads zero and `matrix_world` holds the transform

Use bim.md for IFC.

## [02]-[RHINO]

Rhino files reach Blender through `.3dm` for layers and `.glb` for materials, and Blender reaches Rhino through `.glb`:
1. Rhino layers arrive as collections through `import_3dm` with `import_layers_as_empties=False`, each slot linked `OBJECT` to its layer material
2. Rhino materials arrive through its `.glb` with display-encoded emission and no `doubleSided`, Blender reading their specular at half
3. Blender objects reach Rhino as a `.glb` with collections as `Scene Collection::<collection>` layers

```python
# [EXECUTE_BLENDER_CODE] Rhino glTF import with Rhino's specular, emission, and culling undone
import bpy
from mathutils import Color

before = set(bpy.data.materials)
bpy.ops.import_scene.gltf(filepath="<file>.glb")
for material in set(bpy.data.materials) - before:
    material.use_backface_culling = False
    shader = next(node for node in material.node_tree.nodes if node.type == "BSDF_PRINCIPLED")
    shader.inputs["Specular IOR Level"].default_value *= 2
    emission = shader.inputs["Emission Color"].default_value
    emission[:3] = Color(emission[:3]).from_srgb_to_scene_linear()
result = {"materials": sorted(m.name for m in set(bpy.data.materials) - before)}
```

```python
# [EXECUTE_BLENDER_CODE] Scene to <file>.glb for Rhino, collections as layers and objects named after themselves
import bpy

for obj in bpy.data.objects:
    if obj.data is not None and obj.data.users == 1:
        obj.data.name = obj.name
result = {"status": sorted(bpy.ops.export_scene.gltf(filepath="<file>.glb", export_format="GLB", export_hierarchy_full_collections=True))}
```

- `import_3dm` keeps base color, metallic, roughness, specular, IOR, transmission, emission, and alpha, and drops coat, sheen, and subsurface
- `import_3dm` loads embedded images alone, reads data images as `sRGB`, and connects no normal image
- Rhino names objects from a glTF after their meshes, the rename before export keeps the object names
- Rhino reads an unwritten default specular as 1.0 and alpha into base color, and reads no emission strength, sheen, or subsurface
- File > Export > glTF takes the `Interchange` operator preset holding `export_format` `GLB` and `export_hierarchy_full_collections`

## [03]-[AUTHORING]

`rhino3dm` and `ezdxf` wheels of installed extensions write `.3dm` and `.dxf` files through `run` or `call`:

```python
# [HEADLESS_CALL] Feet .3dm holding one box mesh <name> on layer <layer>, written to <file>.3dm
import itertools

import rhino3dm

model = rhino3dm.File3dm()
model.Settings.ModelUnitSystem = rhino3dm.UnitSystem.Feet
layer = rhino3dm.Layer()
layer.Name = "<layer>"
attributes = rhino3dm.ObjectAttributes()
attributes.LayerIndex = model.Layers.Add(layer)
attributes.Name = "<name>"
mesh = rhino3dm.Mesh()
for x, y, z in itertools.product((0, <x>), (0, <y>), (0, <z>)):
    mesh.Vertices.Add(x, y, z)
for face in ((0, 2, 6, 4), (1, 5, 7, 3), (0, 4, 5, 1), (2, 3, 7, 6), (0, 1, 3, 2), (4, 6, 7, 5)):
    mesh.Faces.AddFace(*face)
mesh.Normals.ComputeNormals()
model.Objects.AddMesh(mesh, attributes)
result = {"written": model.Write("<file>.3dm", 8)}
```

```python
# [HEADLESS_CALL] Inch DXF holding a closed <x> by <y> outline on layer <layer>, written to <file>.dxf
import ezdxf

doc = ezdxf.new("R2018")
doc.units = ezdxf.units.IN
doc.layers.add("<layer>")
doc.modelspace().add_lwpolyline([(0, 0), (<x>, 0), (<x>, <y>), (0, <y>)], close=True, dxfattribs={"layer": "<layer>"})
doc.saveas("<file>.dxf")
result = {"insunits": doc.header["$INSUNITS"]}
```

- Objects on no layer fail `import_3dm` with `KeyError`, each object takes `ObjectAttributes.LayerIndex`
- Materials a `rhino3dm` file holds import as none, materials cross through `.glb`
- `doc.units` sets `$INSUNITS`, the unit `cad2cube_dxf` reads

## [04]-[BATCH]

`convert` imports each file into an empty factory scene with a world and exports it in a session, and the live session answers `LiveSession`:

```python
# [HEADLESS_CALL] Files to glTF binary under .artifacts/blender/convert/<name>/, collections as Rhino layers
from convert import convert
from results import as_result

options = {
    "wm.obj_import": {"global_scale": 0.0254},
    "import_3dm.some_data": {"import_layers_as_empties": False},
    "import_scene.cad2cube_dxf": {"recenter_mode": "NONE"},
    "cityjson.import_file": {"clean_scene": False},
    "export_scene.gltf": {"export_hierarchy_full_collections": True},
}
result = as_result(convert("<name>", "export_scene.gltf", ("<dir>/<part>.step", "<dir>/<model>.obj", "<dir>/<model>.3dm", "<dir>/<plan>.dxf", "<dir>/<city>.json"), options=options))
```

1. `headless.py start <scratch>.blend <name>`, then the snippet through `call`
2. Read each `Converted` extent against the source's stated size, and `empty` for meshes the exporter left out
3. Read `convert.log` under a `---` line naming a source for its importer and exporter output
4. `headless.py stop <name>`

- `options` maps an importer or exporter id to its keyword arguments, and an importer it names reads the files its filter covers
- Files no filter covers take the one named importer no other file's filter matches (`wm.usd_import` for `.usdc`)
- C importers read their filter's suffixes and Python importers their file handler's, `cad2cube_dxf` and `cityjson.import_file` read once named
- `Converted` holds the importer, object count, `empty` mesh names, world extent in meters, and every file the exporter wrote
- `convert.json` beside the outputs holds the batch, one case per file in input order, and each run clears the folder first
- `NoImporter`, `AmbiguousImporter`, `SharedStem`, and `Failed` name files the batch skipped, every other file converts
- `UnknownOperator` names ids of neither role and `UnknownOptions` properties an operator does not declare, and the batch runs no file
