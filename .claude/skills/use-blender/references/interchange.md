# [INTERCHANGE]

Covers CAD data entering and leaving Blender.

## [01]-[ROUTES]

Importers run in exec form with operator defaults, and importer preferences seed the File > Import dialog alone:

| [INDEX] | [TASK]                             | [ROUTE]                                                                | [SESSION]         |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------- | :---------------- |
|  [01]   | Rhino model into Blender           | Rhino saves render meshes, then `import_3dm.some_data`                 | Background or GUI |
|  [02]   | STEP or IGES part or assembly      | `import_scene.step`, tessellation from the extension's preferences     | Background or GUI |
|  [03]   | DXF drawing                        | `$INSUNITS` read through `ezdxf`, then `import_scene.cad2cube_dxf`     | Background or GUI |
|  [04]   | 3ds Max scene                      | `import_scene.max` with `scale_objects` of the file's system unit      | Background or GUI |
|  [05]   | OBJ, STL, PLY, FBX, glTF, USD mesh | Stock importer with the source unit as scale                           | Background or GUI |
|  [06]   | SVG drawing as curves              | `import_curve.svg` with a unit factor, or Grease Pencil import         | Background or GUI |
|  [07]   | Blender scene for Rhino            | `.3dm` written through the bundled `rhino3dm` wheel                    | Background        |
|  [08]   | Attribute table out                | `export.attrio_csv` on the point domain, Python `csv` on other domains | Background or GUI |
|  [09]   | Attribute table in                 | Geometry Nodes Import CSV node                                         | Background or GUI |
|  [10]   | Exchange confirmed                 | Snapshot, export, reimport, compare                                    | Background        |

Bundled wheels (`rhino3dm`, `ezdxf`, `cascadio`) import in any Blender Python run on the configured user folder.

## [02]-[IMPORT_READ]

Every import confirms through one read of added objects, their collections, and world extents from evaluated geometry, curve `bound_box` values spanning -1 to 1 on axes their points never reach:

```python
import bpy

before = set(bpy.data.objects)
status = bpy.ops.<importer>(filepath="<file>", <arguments>)
added = [obj for obj in bpy.data.objects if obj not in before]
depsgraph = bpy.context.evaluated_depsgraph_get()


def extent(obj: bpy.types.Object) -> list[list[float]] | None:
    evaluated = obj.evaluated_get(depsgraph)
    mesh = evaluated.to_mesh() if obj.type in {"MESH", "CURVE", "FONT", "SURFACE"} else None
    points = [obj.matrix_world @ vertex.co for vertex in mesh.vertices] if mesh else []
    evaluated.to_mesh_clear()
    return [[round(min(p[i] for p in points), 4) for i in range(3)], [round(max(p[i] for p in points) - min(p[i] for p in points), 4) for i in range(3)]] if points else None


instances = [(i.parent.name, i.object.name, [round(v, 4) for v in i.matrix_world.translation]) for i in depsgraph.object_instances if i.is_instance and i.parent.original in added]
result = {"status": sorted(status), "objects": {obj.name: (obj.type, [c.name for c in obj.users_collection], extent(obj)) for obj in added}, "instances": instances}
```

- Extents off by 1000, 25.4, 12, or 3.2808 against a dimension the source states mark a unit error, and swapped extents an axis error
- `MESH` objects with no vertices mark geometry the source stored without a mesh
- Set differences of `bpy.data.objects` name added objects whatever the selection holds later

## [03]-[RHINO_IN]

`import_3dm` meshes Breps and extrusions from render meshes the `.3dm` stores, and Rhino writes render meshes only for objects a shaded viewport drew. Before the save, the Rhino side creates render meshes and replaces closed polycurves with NURBS curves (`use-rhino` `run_csharp`, then `save_doc`):

```csharp
var doc = __rhino_doc__;
var all = new Rhino.DocObjects.ObjectEnumeratorSettings { HiddenObjects = true, LockedObjects = true };
var parameters = doc.GetMeshingParameters(doc.MeshingParameterStyle);
var members = doc.InstanceDefinitions.Where(d => !d.IsDeleted).SelectMany(d => d.GetObjects());
foreach (var obj in doc.Objects.GetObjectList(all).Concat(members))
    obj.CreateMeshes(Rhino.Geometry.MeshType.Render, parameters, false);
foreach (var obj in doc.Objects.GetObjectList(all).Where(o => o.Geometry is Rhino.Geometry.PolyCurve { IsClosed: true }).ToList())
    doc.Objects.Replace(obj.Id, ((Rhino.Geometry.Curve)obj.Geometry).ToNurbsCurve());
```

Blender imports in Object Mode, layers as collections:

```python
bpy.ops.import_3dm.some_data(filepath="<file>.3dm", import_layers_as_empties=False)
```

- Lengths scale from the model unit to meters, divided by `scene.unit_settings.scale_length`
- Collection `<file stem>` holds `Layers` (each Rhino layer a nested collection), `NamedViews`, and a hidden `Instance Definitions`
- Blocks become collection instances named `<block>_Instance` with the Rhino name in custom property `rhname`
- Objects hold `rhid`, `rhname`, `rhmatid`, and one custom property per Rhino user string
- Render materials become Principled materials named after the Rhino material
- SubD objects import their cage with a `SUBSURF` modifier, and text and dimensions import as leader curves with a `FONT` child
- Hidden and locked layers import visible, `import_hidden_layers=False` and `import_hidden_objects=False` leaving them out
- With reimports matching data by `rhid` and adding block instances and text again, a refresh removes the `<stem>` collection tree first
- Each `.3dm` imports into its own scene, a second file's layers nesting inside the first file's `Layers` collection

Confirmation read: the import read's extents against Rhino bounding boxes in meters, layer collection object counts against `list_objects` per layer, and no `MESH` with zero vertices.

## [04]-[STEP_IN]

STEP Importer converts the file through `cascadio` and imports the result into the active collection, tessellation reading the extension's preferences:

```python
prefs = bpy.context.preferences.addons["bl_ext.blender_org.step_importer"].preferences
prefs.tol_preset = "FINE"
bpy.ops.import_scene.step(filepath="<file>.step", use_assembly_collections=True)
```

- STEP lengths arrive in meters from the file's unit, and IGES lengths take `scale=0.001`
- Default `up_axis="Y"` keeps a Z-up CAD model upright
- `tol_preset` (`DRAFT` to `ULTRA_FINE`, `CUSTOM`) writes `tol_linear` and `tol_angular`
- Calls restore the previous preset after the import, preferences saving at quit
- Assemblies keep their tree as empties, and `use_assembly_collections=True` adds one collection per assembly
- With objects holding `QUATERNION` rotation, rotation reads come from `matrix_world` or `rotation_quaternion`
- `step.regenerate_part` re-tessellates selected parts from their `step_source_file` and `step_node_path` at `window_manager.step_regen` settings
- `merge_objects=True` joins every mesh into one, keeping each part's material in its slots

Confirmation read: the import read's extents against the part dimensions.

## [05]-[DXF_IN]

With CAD2Cube scaling by `$INSUNITS` in `units_mode="AUTO"` and reading unitless files as millimeters and US survey feet (21) as yards, the header reads through `ezdxf` first and scale passes as meters per unit:

```python
import bpy
import ezdxf

meters = {1: 0.0254, 2: 0.3048, 4: 0.001, 5: 0.01, 6: 1.0, 21: 1200 / 3937}
units = ezdxf.readfile("<file>.dxf").header.get("$INSUNITS", 0)
bpy.ops.import_scene.cad2cube_dxf(filepath="<file>.dxf", units_mode="MANUAL", manual_scale=meters.get(units, <meters per the drawing's stated unit>), recenter_mode="NONE", frame_after_import=False, color_by_layer=False, import_text=True)
```

- `recenter_mode="NONE"` keeps coordinates, the default `BBOX` moving the drawing's box center to the origin
- `frame_after_import=False` and `color_by_layer=False` leave viewports as found, the defaults reframing and recoloring every 3D view
- Layers become collections under `<file stem>`
- `skip_hidden_layers=True` (default) leaves out layers that are off, frozen, or locked
- `layer_filter` (`"WALL,!DIM"`) selects layers by name
- `block_mode="INSTANCES"` (default) builds block collections under a hidden `<stem>_Blocks` with `INS_<block>` empties
- `block_mode="EXPANDED"` writes inserts as geometry
- `auto_join_curves=True` (default) joins touching lines of one layer into `<layer>_Joined`
- `import_linetypes=False` keeps dashed entities whole for length reads
- Arcs, circles, and bulges import as exact `BEZIER` splines, lines and polylines as `POLY`, hatches as faces

Georeferenced drawings lose precision in float32 conversion and move to a local origin in `ezdxf` before the import, the offset kept on the scene:

```python
from ezdxf import bbox

doc = ezdxf.readfile("<file>.dxf")
modelspace = doc.modelspace()
origin = bbox.extents(modelspace).extmin
for entity in modelspace:
    entity.translate(-origin.x, -origin.y, 0)
doc.saveas("<local>.dxf")
bpy.context.scene["dxf_origin"] = (origin.x, origin.y)
```

Confirmation read: the import read's extents against a dimension the drawing states, and collection names against `[layer.dxf.name for layer in doc.layers]`.

## [06]-[MAX_IN]

Import Autodesk MAX reads no unit from the file and multiplies system units by `scale_objects` in meters per unit (0.001 millimeters, 0.01 centimeters, 0.0254 inches, 0.3048 feet):

```python
bpy.ops.import_scene.max(filepath="<file>.max", scale_objects=0.0254, use_collection=True)
```

`object_filter` (`MATERIAL`, `UV`, `PRIMITIVE`, `EMPTY`, `ARMATURE`) selects what imports.

Confirmation read: the import read's extents against a dimension the source states.

## [07]-[MESH_FORMATS_IN]

Each stock importer takes the source unit in its own argument:

| [INDEX] | [FORMAT] | [OPERATOR]          | [UNIT]                         | [AXES]                     |
| :-----: | :------- | :------------------ | :----------------------------- | :------------------------- |
|  [01]   | OBJ      | `wm.obj_import`     | `global_scale` meters per unit | Defaults read Rhino's Y-up |
|  [02]   | STL, PLY | `wm.stl_import`     | `global_scale` meters per unit | Defaults keep Z-up         |
|  [03]   | FBX      | `import_scene.fbx`  | File unit applied              | File axes applied          |
|  [04]   | GLB      | `import_scene.gltf` | Meters                         | Y-up converted to Z-up     |
|  [05]   | USD      | `wm.usd_import`     | `metersPerUnit` applied        | `upAxis` applied           |

Rhino exports differ by format:
- OBJ and STL hold model-unit values (`global_scale=0.3048` for feet)
- FBX and USD keep layers as parent empties
- GLB loses layers and block names
- `.3dm` keeps layers, curves, and blocks

Confirmation read: the import read's extents against the source's stated size.

## [08]-[SVG_IN]

With `import_curve.svg` sizing a document as millimeters times its root unit, millimeter documents import at size and others take a factor on added objects:

```python
from mathutils import Matrix

before = set(bpy.data.objects)
bpy.ops.import_curve.svg(filepath="<file>.svg")
factor = {"mm": 1.0, "cm": 0.1, "in": 1 / 25.4, "px": 90 / 96}["<root height unit, px when none>"]
for obj in set(bpy.data.objects) - before:
    obj.matrix_world = Matrix.Scale(factor, 4) @ obj.matrix_world
```

- Objects take the element `id`, SVG Y flips with the `viewBox` bottom on Y 0, and filled shapes become filled 2D curves
- `wm.grease_pencil_import_svg(scale=1.0, recenter_bounds=False)` imports every unit at size into one Grease Pencil object in the XZ plane
- `wm.grease_pencil_import_svg` default `scale` of 10 enlarges the drawing

Confirmation read: the import read's extents against the drawing's stated dimension.

## [09]-[RHINO_OUT]

`.3dm` written through `rhino3dm` keeps collections as layers, names, curves, blocks, materials, and custom properties. Every object takes `ObjectAttributes` with the `LayerIndex` of a layer the file holds, `import_3dm` raising `KeyError` on one without:

```python
import bpy
import rhino3dm

model = rhino3dm.File3dm()
model.Settings.ModelUnitSystem = rhino3dm.UnitSystem.Meters
layer = rhino3dm.Layer()
layer.Name = "<collection>"
child = rhino3dm.Layer()
child.Name = "<child collection>"
child.ParentLayerId = model.Layers.FindIndex(model.Layers.Add(layer)).Id
attributes = rhino3dm.ObjectAttributes()
attributes.LayerIndex = model.Layers.Add(child)
attributes.Name = "<object>"
attributes.SetUserString("<property>", "<value>")
material = rhino3dm.Material()
material.Name = "<material>"
material.ToPhysicallyBased()
material.PhysicallyBased.BaseColor = (<r>, <g>, <b>, 1.0)
attributes.MaterialIndex = model.Materials.Add(material)
attributes.MaterialSource = rhino3dm.ObjectMaterialSource.MaterialFromObject
model.Objects.AddMesh(<rhino3dm.Mesh>, attributes)
model.Write("<file>.3dm", 8)
```

- Meshes come from the evaluated object in world positions, triangles and quads through `Mesh.Faces.AddFace`, n-gons through `loop_triangles`
- Bezier splines become one cubic `rhino3dm.NurbsCurve(3, False, 4, 3 * segments + 1)` over `[co, handle_right, next handle_left, next co, ...]`
- Bezier curve knots run `0, 0, 0, 1, 1, 1, ...`
- Poly splines become `rhino3dm.PolylineCurve`, the first point repeated when cyclic
- Instanced collections become one `InstanceDefinitions.Add(name, "", "", "", base_point, meshes, attributes)` each
- Collection instance empties become `Objects.AddInstanceObject(rhino3dm.InstanceReference(definition.Id, transform), attributes)`
- `transform` holds the empty's `matrix_world`
- Rhino reads a meters file at size in any document unit

Mesh-only exchanges take GLB once each mesh holds a material and mesh data holds its object's name, Rhino naming objects after mesh data:

```python
grey = bpy.data.materials.new("<Default>")
for obj in bpy.context.scene.objects:
    if obj.type == "MESH" and not obj.data.materials:
        obj.data.materials.append(grey)
    if obj.data is not None and obj.data.users == 1:
        obj.data.name = obj.name
bpy.ops.export_scene.gltf(filepath="<file>.glb", export_format="GLB", export_hierarchy_full_collections=True, export_apply=True, use_visible=True)
```

FBX for Rhino takes `axis_forward="Y"` and `axis_up="Z"`, and OBJ takes `global_scale` in document units per meter with `forward_axis="Y"` and `up_axis="Z"`. With Rhino's USD import aborting Rhino on a Blender `.usdc`, Rhino exchanges take `.3dm` or GLB.

Confirmation read: `use-rhino` `open_doc` of the file, then `run_csharp` listing each object's layer path, name, bounding box, render material, and user strings against the Blender scene.

## [10]-[ATTRIBUTES]

Attrio CSV writes the active object's evaluated point attributes per frame, settings on `scene.attrio_csv_settings`:

```python
settings = bpy.context.scene.attrio_csv_settings
settings.export_path, settings.subfolder, settings.base_filename = "<dir>", "<subfolder>", "<name>"
settings.multi_frame = False
bpy.context.view_layer.objects.active = <object>
settings.domain = "POINT"
bpy.ops.attrio.refresh_attributes()
for item in settings.attribute_list:
    item.use = not item.name.startswith(".")
bpy.ops.export.attrio_csv()
```

- Exports write `<export_path>/<subfolder>/<base_filename>_<frame:04>.csv`, vectors as `<name>_x`, `_y`, `_z` columns
- Attrio rows count vertices on every domain
- Face and edge tables write through Python `csv` from `attributes["<name>"].data.foreach_get` on the evaluated mesh

With Attrio's import operators loading nothing on macOS, tables come in through the Geometry Nodes Import CSV node, one attribute per column, positions set from named columns:

```python
group = bpy.data.node_groups.new("<CSV Points>", "GeometryNodeTree")
group.interface.new_socket("Geometry", in_out="OUTPUT", socket_type="NodeSocketGeometry")
nodes, links = group.nodes, group.links
reader = nodes.new("GeometryNodeImportCSV")
reader.inputs["Path"].default_value = "<file>.csv"
combine = nodes.new("ShaderNodeCombineXYZ")
for axis, socket in zip("xyz", combine.inputs):
    column = nodes.new("GeometryNodeInputNamedAttribute")
    column.data_type = "FLOAT"
    column.inputs["Name"].default_value = f"<prefix>_{axis}"
    links.new(column.outputs["Attribute"], socket)
place = nodes.new("GeometryNodeSetPosition")
links.new(reader.outputs[0], place.inputs["Geometry"])
links.new(combine.outputs[0], place.inputs["Position"])
links.new(place.outputs[0], nodes.new("NodeGroupOutput").inputs[0])
holder = bpy.data.objects.new("<CSV Points>", bpy.data.meshes.new("<CSV Points>"))
bpy.context.collection.objects.link(holder)
holder.modifiers.new("<CSV>", "NODES").node_group = group
```

Whole-number columns load as `INT`, a `FLOAT` Named Attribute converting them. Values for an existing mesh write through Python `csv` and `attributes.new(name, type, domain).data.foreach_set`.

Confirmation read: the header row and row count against the evaluated vertex count out, and the evaluated `holder.evaluated_get(depsgraph).evaluated_geometry().pointcloud` point count and attribute names against the file in.

## [11]-[ROUND_TRIP]

Round trips compare the scene before export with the file read back:
1. Snapshot each object: type, import-read extent, collections, materials, custom properties, and each instance's location and source
2. Export through the Rhino-out sequence
3. Open and save the file in Rhino through `use-rhino` when the exchange includes Rhino editing
4. Import into an empty scene (`wm.read_homefile(use_empty=True)`) with `import_3dm.some_data(import_layers_as_empties=False)`
5. Compare snapshots by name

Exchange-owned differences:
- Instances return as `<block>_Instance` with the source name in `rhname`
- Materials return empty from a `rhino3dm` file and whole after a Rhino save
- Mesh objects gain one empty material slot
- Blocks Rhino imported across units hold the unit scale on the instance empty

Confirmation read: the comparison lists no difference outside exchange-owned differences, extents matching to four decimals.
