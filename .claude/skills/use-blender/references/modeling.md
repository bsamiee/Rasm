# [MODELING]

Meshes built in code and the objects, modifiers, and collections holding them.

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

- `bmesh.ops.create_cube(bm, size=1.0, matrix=Matrix.Diagonal((x, y, z, 1)))` builds an `x` by `y` by `z` box centered on the origin
- Circular primitives take `vertices=64` or more, facets meeting under 5.73° draw no wireframe edge at `overlay.wireframe_threshold` 0
- `mesh.primitive_quad_sphere_add(segments=, radius=)` builds an all-quad sphere
- `object.join()` under a selection override with `active_object=<target>` merges the selected objects into the target
- `object.transform_apply(location=False, rotation=False, scale=True)` under a selection override bakes object scale into the mesh

## [02]-[MODIFIERS]

- Stacks evaluate top down, `object.modifier_apply(modifier=)` under `temp_override(object=, active_object=)` bakes one modifier into the mesh
- Applies below the top of the stack print `Info: Applied modifier was not first` and bake onto the unmodified mesh, and the modifiers above stay
- Cutters keep cutting while `hide_set(True)` hides them, `hide_render` True keeps them out of renders, and captures skip them
- `bpy.ops.object.shade_auto_smooth(angle=<radians>)` adds a `Smooth by Angle` modifier

## [03]-[EDIT_MODE]

- `object.mode_set(mode="EDIT")` opens Edit Mode on `view_layer.objects.active`, and `bmesh.from_edit_mesh(<obj>.data)` returns the mesh it holds
- `bmesh.update_edit_mesh(<obj>.data)` writes the bmesh into the edit mesh the evaluated object reads in Edit Mode
- Calls return through `object.mode_set(mode="OBJECT")` before `result`, `obj.data` reads the edit from then on

## [04]-[ORGANIZATION]

- Operators link new objects into `bpy.context.collection`, the view layer's active collection, and `bpy.data.objects.new` links none
- `matrix_world` of an object placed in the same call holds its transform once `bpy.context.view_layer.update()` evaluates it
