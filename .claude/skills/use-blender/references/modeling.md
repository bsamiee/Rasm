# [MODELING]

Meshes built from dimensions in code, kept in collections, and changed in place, each change closing on evaluated values and a capture.

## [01]-[NEW_PART]

New parts build in one call from dimensions in meters, with the evaluated dimensions and a capture in `result`:

```python
# [EXECUTE_BLENDER_CODE] Room of slab, walls cut by a door and a window, and a column row, each member from its dimensions
import bmesh
import bpy
from capture import capture
from mathutils import Matrix, Vector
from results import as_result

INCH = 0.0254
FOOT = 12 * INCH
part, cutters = bpy.data.collections.new("Room"), bpy.data.collections.new("Room Cutters")
bpy.context.scene.collection.children.link(part)
part.children.link(cutters)
bpy.context.view_layer.layer_collection.children[part.name].children[cutters.name].hide_viewport = True
cutters.hide_render = True


def solid(
    name: str, boxes: list[tuple[tuple[float, float, float], tuple[float, float, float]]], owner: bpy.types.Collection, location: tuple[float, float, float], parent: bpy.types.Object | None = None
) -> bpy.types.Object:
    mesh, bm = bpy.data.meshes.new(name), bmesh.new()
    for size, corner in boxes:
        bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Translation(Vector(corner) + Vector(size) / 2) @ Matrix.Diagonal((*size, 1.0)))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.parent, obj.location = parent, location
    owner.objects.link(obj)
    return obj


length, depth, height, wall, level = 24 * FOOT, 16 * FOOT, 10 * FOOT, 6 * INCH, 8 * INCH
solid("Slab", [((length, depth, level), (0.0, 0.0, 0.0))], part, (0.0, 0.0, 0.0))
sides = ((length, wall, height), (wall, depth - 2 * wall, height))
walls = solid("Walls", [(sides[0], (0.0, 0.0, 0.0)), (sides[0], (0.0, depth - wall, 0.0)), (sides[1], (0.0, wall, 0.0)), (sides[1], (length - wall, wall, 0.0))], part, (0.0, 0.0, level))
solid("Door", [((3 * FOOT, 2 * wall, 7 * FOOT), (0.0, 0.0, 0.0))], cutters, (4 * FOOT, -wall / 2, 0.0), walls)
solid("Window", [((6 * FOOT, 2 * wall, 4 * FOOT), (0.0, 0.0, 0.0))], cutters, (14 * FOOT, -wall / 2, 3 * FOOT), walls)
openings = walls.modifiers.new("Openings", "BOOLEAN")
openings.operand_type, openings.collection, openings.solver = "COLLECTION", cutters, "EXACT"
column = solid("Columns", [((8 * INCH, 8 * INCH, height), (0.0, 0.0, 0.0))], part, (2 * FOOT, depth + 4 * FOOT, level))
row = column.modifiers.new("Row", "ARRAY")
row.count, row.use_relative_offset, row.use_constant_offset, row.constant_offset_displace = 5, False, True, (5 * FOOT, 0.0, 0.0)
depsgraph = bpy.context.evaluated_depsgraph_get()
feet = {o.name: [round(d / FOOT, 4) for d in o.evaluated_get(depsgraph).dimensions] for o in part.all_objects}
result = {"feet": feet, "iso": as_result(capture("room-iso", objects=tuple(o.name for o in part.objects)))}
```

- `solid` puts each object origin at its member's lower corner for `location` to place, its boxes forming one mesh with no join
- `bpy.data.collections.new` makes an unlinked collection, and `scene.collection.children.link` places it
- Children given a parent before their location take it in parent space and move with that parent
- One Boolean of `operand_type` `COLLECTION` cuts with every object in the cutter collection
- Cutters stay hidden through their layer collection's `hide_viewport` and out of renders through `Collection.hide_render`
- Arrays space by `constant_offset_displace` in meters with `use_relative_offset` off
- Round members at 64 segments draw no facet edge under a wireframe threshold of 0, which startup viewports hold
- `mesh.shade_smooth()` then `mesh.set_sharp_from_angle(angle=radians(30))` keeps cap rims sharp with no modifier
- Members stand on the startup `Ground` from z 0 up, a `WIRE` plane renders draw and captures leave out
- `feet` compares each evaluated member with its declared dimensions, and `room-iso` becomes the capture baseline

## [02]-[EXISTING_PART]

Existing parts are read, then changed in one call between a baseline and its comparison:
1. Read collections and parents through `get_objects_summary`, one object's stacks through `get_object_detail_summary`
2. Edit through the edit mesh while its object is in Edit Mode, else through a `bmesh` of mesh data
3. Move a cutter to move its opening, which its Boolean recomputes
4. Read `changes` and the capture comparison

```python
# [EXECUTE_BLENDER_CODE] Walls raised from 10' to 12' and the window moved 2' along the wall, against a baseline taken first
import bmesh
import bpy
from capture import capture
from results import as_result
from snapshot import snapshot

FOOT = 0.3048
obj, names = bpy.data.objects["Walls"], ("Walls", "Slab")
before = {"state": as_result(snapshot("room-before", objects=names)), "iso": as_result(capture("room-before-iso", objects=names))}
mesh = obj.data
editing = mesh.is_editmode
bm = bmesh.from_edit_mesh(mesh) if editing else bmesh.new()
if not editing:
    bm.from_mesh(mesh)
bmesh.ops.translate(bm, verts=[v for v in bm.verts if v.co.z > 5 * FOOT], vec=(0.0, 0.0, 2 * FOOT))
if editing:
    bmesh.update_edit_mesh(mesh)
else:
    bm.to_mesh(mesh)
    bm.free()
obj.modifiers["Openings"].collection.objects["Window"].location.x += 2 * FOOT
result = {"before": before, "changes": as_result(snapshot("room-after", objects=names, since="room-before")), "iso": as_result(capture("room-after-iso", objects=names, since="room-before-iso"))}
```

- `to_mesh` raises `ValueError` on a mesh in Edit Mode, and `update_edit_mesh` writes what evaluated objects and snapshots read
- `obj.data` holds an Edit Mode edit once `object.mode_set(mode="OBJECT")` runs
- `changes` names raised bounds and a new geometry hash, and `outside` true marks walls past their baseline frame

Use captures.md for each comparison reading and its next step.

Existing objects merge into one with their stacks baked, `join` taking each other object's mesh without its modifiers:

```python
# [EXECUTE_BLENDER_CODE] <Object> merged into <Target> with its modifiers applied, its orphaned mesh removed
import bpy

target = bpy.data.objects["<Target>"]
others = [bpy.data.objects[name] for name in ("<Object>",)]
for obj in others:
    for name in [modifier.name for modifier in obj.modifiers]:
        with bpy.context.temp_override(active_object=obj, selected_objects=[obj], selected_editable_objects=[obj]):
            bpy.ops.object.modifier_apply(modifier=name)
meshes = [obj.data for obj in others]
with bpy.context.temp_override(active_object=target, selected_objects=[target, *others], selected_editable_objects=[target, *others]):
    status = bpy.ops.object.join()
bpy.data.batch_remove(meshes)
result = {"status": sorted(status), "faces": len(target.data.polygons)}
```

- Stacks bake top down, one `modifier_apply` per modifier name in stack order
- `join` keeps its target's modifiers, removes every other object, and leaves their meshes at 0 users for `batch_remove`
- Placed objects keep their world transform under a new parent through `child.matrix_parent_inverse = parent.matrix_world.inverted()`
- Objects placed in the same call take `bpy.context.view_layer.update()` before that read of `matrix_world`
