# [MODELING]

Meshes build from dimensions in code, stay in collections, and change in place, each change closing on evaluated values and a capture.

## [01]-[NEW_PART]

New parts build in one call from dimensions in meters, the data API linking every object into a named collection:
1. Convert the given dimensions to meters at 1/64 inch
2. Build each member as boxes of one mesh with its origin at the member's lower corner, `location` placing it
3. Cut openings with one Boolean of `operand_type` `COLLECTION` over a cutter collection its layer collection hides
4. Repeat members through an Array at a constant offset
5. Return the evaluated dimensions in feet and an iso capture of the part

```python
# [EXECUTE_BLENDER_CODE] Slab, walls cut by openings, and a column row in <Collection>, every member from its dimensions in meters
import bmesh
import bpy
from capture import capture
from mathutils import Matrix, Vector
from results import as_result

type Box = tuple[tuple[float, float, float], tuple[float, float, float]]
length, depth, height, wall, level = <length>, <depth>, <height>, <wall>, <level>
openings = {"<Opening>": ((<width>, 2 * wall, <tall>), (<x>, -wall / 2, <z>))}
side, count, spacing, start = <side>, <count>, <spacing>, (<x>, <y>)
part, cutters = bpy.data.collections.new("<Collection>"), bpy.data.collections.new("<Collection> Cutters")
bpy.context.scene.collection.children.link(part)
part.children.link(cutters)
bpy.context.view_layer.layer_collection.children[part.name].children[cutters.name].hide_viewport = True
cutters.hide_render = True


def solid(name: str, boxes: list[Box], owner: bpy.types.Collection, location: tuple[float, float, float], parent: bpy.types.Object | None = None) -> bpy.types.Object:
    mesh, bm = bpy.data.meshes.new(name), bmesh.new()
    for size, corner in boxes:
        bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Translation(Vector(corner) + Vector(size) / 2) @ Matrix.Diagonal((*size, 1.0)))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.parent, obj.location = parent, location
    owner.objects.link(obj)
    return obj


solid("<Slab>", [((length, depth, level), (0.0, 0.0, 0.0))], part, (0.0, 0.0, 0.0))
sides = ((length, wall, height), (wall, depth - 2 * wall, height))
walls = solid("<Walls>", [(sides[0], (0.0, 0.0, 0.0)), (sides[0], (0.0, depth - wall, 0.0)), (sides[1], (0.0, wall, 0.0)), (sides[1], (length - wall, wall, 0.0))], part, (0.0, 0.0, level))
for name, (size, location) in openings.items():
    solid(name, [(size, (0.0, 0.0, 0.0))], cutters, location, walls)
cut = walls.modifiers.new("Openings", "BOOLEAN")
cut.operand_type, cut.collection, cut.solver = "COLLECTION", cutters, "EXACT"
column = solid("<Columns>", [((side, side, height), (0.0, 0.0, 0.0))], part, (*start, level))
row = column.modifiers.new("Row", "ARRAY")
row.count, row.use_relative_offset, row.use_constant_offset, row.constant_offset_displace = count, False, True, (spacing, 0.0, 0.0)
depsgraph = bpy.context.evaluated_depsgraph_get()
result = {"feet": {o.name: [round(d / 0.3048, 4) for d in o.evaluated_get(depsgraph).dimensions] for o in part.all_objects}, "iso": as_result(capture("<name>-iso", objects=tuple(o.name for o in part.objects)))}
```

- Boxes of one `solid` form one mesh with no join, and `bpy.data.objects.new` keeps the user's active object and selection
- Children take a parent before their location, placing them in parent space and moving them with that parent
- Booleans keep cutting under `hide_set(True)`, and `Collection.hide_render` keeps cutters out of renders
- Evaluated meshes count the faces a Boolean cuts in `data.polygons`
- Round members take `bmesh.ops.create_cone(bm, cap_ends=True, segments=64, radius1=<r>, radius2=<r>, depth=<h>)`, centered on the origin
- 64 segments draw no facet edge under the wireframe threshold of 0 startup viewports hold
- `mesh.shade_smooth()` then `mesh.set_sharp_from_angle(angle=radians(30))` keeps the 128 cap rim edges sharp with no modifier
- Members stand from z 0 up on the startup `Ground`, a `WIRE` plane renders and captures leave out

Use configuration.md for unit conversion.

## [02]-[EXISTING_PART]

Existing parts change in one call between a baseline and its comparison:
1. Read collections and parents through `get_objects_summary`, one object's stacks through `get_object_detail_summary`
2. Take the snapshot and capture baselines of the objects the edit reaches
3. Edit through `bmesh.from_edit_mesh` and `update_edit_mesh` while `mesh.is_editmode` reads True, else through a `bmesh` of the mesh data
4. Move a cutter to move its opening, which its Boolean recomputes
5. Read the snapshot `changed` and the capture comparison

```python
# [EXECUTE_BLENDER_CODE] <Walls> raised <rise> m above <cut> m and opening <Opening> moved <shift> m along the wall, against a baseline taken first
import bmesh
import bpy
from capture import capture
from results import as_result
from snapshot import snapshot

obj, names = bpy.data.objects["<Walls>"], ("<Walls>", "<Slab>")
before = {"state": as_result(snapshot("<name>-before", objects=names)), "iso": as_result(capture("<name>-before-iso", objects=names))}
mesh = obj.data
editing = mesh.is_editmode
bm = bmesh.from_edit_mesh(mesh) if editing else bmesh.new()
if not editing:
    bm.from_mesh(mesh)
bmesh.ops.translate(bm, verts=[v for v in bm.verts if v.co.z > <cut>], vec=(0.0, 0.0, <rise>))
if editing:
    bmesh.update_edit_mesh(mesh)
else:
    bm.to_mesh(mesh)
    bm.free()
obj.modifiers["Openings"].collection.objects["<Opening>"].location.x += <shift>
result = {"before": before, "changes": as_result(snapshot("<name>-after", objects=names, since="<name>-before")), "iso": as_result(capture("<name>-after-iso", objects=names, since="<name>-before-iso"))}
```

- `update_edit_mesh` writes the edit mesh evaluated objects and snapshots read, `obj.data` holding the edit after `object.mode_set(mode="OBJECT")`
- Walls take `iso`, axis views showing their openings edge-on against the wall behind them
- `changed` names the raised bounds and a new geometry `shape` hash, a moved cutter changing the hash of the walls it cuts
- Capture `outside` reads True once the walls pass their baseline frame or its clip range

Use SKILL.md for each comparison reading and its next step.

## [03]-[JOINS_AND_PARENTS]

Objects merge into one with their stacks baked, `join` taking each other object's mesh without its modifiers:
1. Give each linked duplicate its own mesh through `obj.data = obj.data.copy()`
2. Apply each modifier in stack order under `temp_override(object=<obj>)`, top down
3. Join under the selection override with the target active
4. Remove the meshes `join` left at 0 users

```python
# [EXECUTE_BLENDER_CODE] <Object> merged into <Target> with its modifiers applied, its orphaned mesh removed
import bpy

target = bpy.data.objects["<Target>"]
others = [bpy.data.objects[name] for name in ("<Object>",)]
for obj in others:
    if obj.data.users > 1:
        obj.data = obj.data.copy()
    for name in [modifier.name for modifier in obj.modifiers]:
        with bpy.context.temp_override(object=obj):
            bpy.ops.object.modifier_apply(modifier=name)
meshes = [obj.data for obj in others]
with bpy.context.temp_override(active_object=target, selected_objects=[target, *others], selected_editable_objects=[target, *others]):
    status = bpy.ops.object.join()
bpy.data.batch_remove(meshes)
result = {"status": sorted(status), "faces": len(target.data.polygons)}
```

- `join` keeps its target's origin and modifiers and removes every other object
- Linked duplicates keep the shared mesh and its modifiers, the copy taking the apply

Placed objects keep their world transform under a new parent:
1. `bpy.context.view_layer.update()` runs after objects placed in the same call
2. `child.parent, child.matrix_parent_inverse = parent, parent.matrix_world.inverted()` parents each child in place
