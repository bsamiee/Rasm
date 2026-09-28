# [MODELING]

Architectural and CAD geometry comes from stated dimensions in code.

## [01]-[MESH]

```python
# [EXECUTE_BLENDER_CODE] Wall from feet-and-inch dimensions with a door cut by a hidden cutter parented to it
import bmesh
import bpy
from mathutils import Matrix

INCH = 0.0254
FOOT = 12 * INCH


def box(name: str, size: tuple[float, float, float], location: tuple[float, float, float]) -> bpy.types.Object:
    mesh, bm = bpy.data.meshes.new(name), bmesh.new()
    bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Diagonal((*size, 1.0)))
    bm.to_mesh(mesh)
    bm.free()
    obj = bpy.data.objects.new(name, mesh)
    obj.location = location
    bpy.context.collection.objects.link(obj)
    return obj


wall = box("<Wall>", (20 * FOOT, 6 * INCH, 10 * FOOT), (0.0, 0.0, 5 * FOOT))
door = box("<Door>", (3 * FOOT, FOOT, 7 * FOOT), (0.0, 0.0, 3.5 * FOOT))
bpy.context.view_layer.update()
door.parent, door.matrix_parent_inverse = wall, wall.matrix_world.inverted()
door.display_type, door.hide_render = "WIRE", True
door.hide_set(True)
cut = wall.modifiers.new("<Door>", "BOOLEAN")
cut.object, cut.solver = door, "EXACT"
evaluated = wall.evaluated_get(bpy.context.evaluated_depsgraph_get())
result = {"dimensions_ft": [d / FOOT for d in evaluated.dimensions], "faces": len(evaluated.data.polygons)}
```

- `bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Diagonal((x, y, z, 1)))` builds a box of those dimensions centered on the origin
- Code edits a mesh in Object Mode through `bm.from_mesh(mesh)` and `bm.to_mesh(mesh)`
- `mesh.from_pydata(vertices, edges, faces)` builds a mesh from lists
- Circular primitives take `vertices=64` or more, and facets meeting under 5.73° draw no edge in the wireframe overlay
- `object.join()` under a selection override with `active_object=<target>` merges the selected objects into the target
- `object.transform_apply(location=False, rotation=False, scale=True)` under a selection override bakes object scale into the mesh

## [02]-[MODIFIERS]

- Stacks evaluate top down, `object.modifier_apply(modifier=)` under `temp_override(object=, active_object=)` bakes one modifier into the mesh
- Applies below the top of the stack print `Info: Applied modifier was not first` and bake onto the unmodified mesh, the modifiers above staying
- Cutters keep cutting while `hide_set(True)` hides them, `hide_render` True keeps them out of renders, and captures skip them

## [03]-[EDIT_MODE]

- `object.mode_set(mode="EDIT")` opens Edit Mode on `view_layer.objects.active`, and `bmesh.from_edit_mesh(<obj>.data)` returns the mesh it holds
- `bmesh.update_edit_mesh(<obj>.data)` writes the bmesh into the edit mesh, `obj.data` still reads the mesh before the edit
- Calls return through `object.mode_set(mode="OBJECT")` before `result`, `obj.data`, modifiers, exports, and snapshots then read the edit

## [04]-[ORGANIZATION]

- `bpy.data.collections.new(<name>)` makes a collection outside every scene, `<parent>.children.link(<collection>)` places it in the tree
- Operators link new objects into `bpy.context.collection`, the view layer's active collection, and `bpy.data.objects.new` links none
- `matrix_world` of an object placed in the same call holds its transform once `bpy.context.view_layer.update()` evaluates it
- Parenting that keeps the child in place sets `child.matrix_parent_inverse = parent.matrix_world.inverted()` beside `child.parent`
