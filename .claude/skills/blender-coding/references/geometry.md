# [GEOMETRY]

## [01]-[EVALUATED_READS]

Evaluated geometry reads through the member working in the space a task needs:
- `obj.ray_cast(origin, direction, distance=, depsgraph=)` and `obj.closest_point_on_mesh(origin, distance=, depsgraph=)` work in object space, face indices addressing the evaluated mesh
- `scene.ray_cast(depsgraph, origin, direction, distance=)` works in world space and returns the hit object with its matrix
- `bm.from_object(obj, depsgraph, cage=, face_normals=, vertex_normals=)` fills a BMesh from the evaluated mesh
- `GeometrySet` holds `mesh`, `mesh_base` (the mesh before final subdivision), `curves`, `pointcloud`, `volume`, and `grease_pencil`, `GeometrySet.from_evaluated_object(evaluated)` building the set `evaluated_geometry()` returns
- `instances_pointcloud()` holds one point per instance, its `instance_transform` attribute in column order with the translation at `[3, :3]` and its `.reference_index` attribute indexing `instance_references()` (`Object`, `Collection`, `GeometrySet`, or `None`)
- `depsgraph.object_instances` takes `len` and iteration with no index, `persistent_id` holding eight integers, zeros outside an instance

## [02]-[BMESH]

BMesh ownership follows the mode:
- Outside Edit Mode, `bmesh.new()` registers `free` on the scope's `ExitStack` and writes back through `bm.to_mesh(mesh)`, a mesh in Edit Mode raising `ValueError`
- In Edit Mode, `bmesh.from_edit_mesh(mesh)` borrows the live mesh, and a mode change or an Edit Mode undo leaves held BMesh and element references raising `ReferenceError`
- In Edit Mode, `mesh.attributes` holds no `position`, and a `position` attribute held from Object Mode reads `name` `""` with no raise
- `bmesh.update_edit_mesh(mesh)` follows each change, a selection-only change calling `bm.select_flush_mode()` first and passing `loop_triangles=False, destructive=False`
- Edit Mode entry takes one object per shared mesh, a second object sharing it reading `mode` `OBJECT` while its mesh reads `is_editmode`, and multi-object work iterates `context.objects_in_mode_unique_data`
- Index reads follow `ensure_lookup_table()` on every new or borrowed BMesh and after every topology edit, `bmesh.ops.create_cube` included, a stale table raising `IndexError`
- New elements join live iterations and hold index -1 until `index_update()`
- Face and vertex normals keep their last computed value through `co` writes, `bm.normal_update()` refreshing them before a read and `to_mesh` deriving mesh normals itself
- UV selection is `BMLoop.uv_select_vert`, `BMLoop.uv_select_edge`, and `BMFace.uv_select`
- `calc_uvs=True` on `bmesh.ops.create_*` writes into an existing UV layer alone, `bm.loops.layers.uv.verify()` creating it first

## [03]-[ATTRIBUTES]

Per-element geometry data sits in attributes, each built-in layer existing once its elements or values do:
- `vertices.add` creates `position`, `loops.add` creates `.corner_vert` and `.corner_edge`, and `update()` derives `.edge_verts` with the edges
- `Curves.add_curves(sizes)` and Grease Pencil `drawing.add_strokes(sizes)` append one curve per listed point count and `PointCloud.resize(count)` sizes a cloud, each holding `position` alone beside Grease Pencil's `curve_type`
- `attributes.new(name, type, domain)` on a taken name, a built-in included, returns a `.001` layer with no raise, and writers test `name in attributes` and read `data_type` and `domain` first
- `uv_layers.new` creates a `FLOAT2` `CORNER` attribute, and Object Mode element selection is the `.select_vert`, `.select_edge`, and `.select_poly` `BOOLEAN` attributes
- The first color attribute becomes both `active_color_name` (painted) and `default_color_name` (rendered), later ones neither, and `active_color` writes leave the rendered layer
- `sharp_face`, `sharp_edge`, `material_index`, and the other built-in layers exist once written, `shade_smooth()` removing `sharp_face` and `set_sharp_from_angle(angle=)` writing `sharp_edge`
- Masks, face sets, bevel weights, and creases are `.sculpt_mask`, `.sculpt_face_set`, `bevel_weight_vert`, `bevel_weight_edge`, `crease_vert`, and `crease_edge`
- `normals_domain` names the domain holding complete normals, `corner_normals` reading it per corner and `normals_split_custom_set` writing custom normals
- Triangles come from `mesh.calc_loop_triangles()`, `loop_triangles` key `vertices` holding 3 `int32` per triangle and `loop_triangle_polygons` key `value` its face
- Vertex groups sit outside original and evaluated attributes, weights reading through `MeshVertex.groups`, and their names belong to the mesh every sharing object lists
- `VertexGroup.remove` takes indices below `len(mesh.vertices)`, a larger index crashing Blender

## [04]-[BUFFERS]

`foreach_get` and `foreach_set` take one flat buffer of exactly `len(collection)` times the member's width, a 2D buffer or another length raising `RuntimeError("internal error setting the array")`. Attribute data and `Image.pixels` copy as one memcpy when the buffer's dtype is the stored type, and every other member or dtype converts per element. Dtype and shape derive from the item class's RNA, enum members reading each item's `value`:

```python
type Buffer = NDArray[np.bool | np.int32 | np.float32]

STORED: MappingProxyType[type[bpy.types.Property], type[np.bool | np.int32 | np.float32]] = MappingProxyType({
    bpy.types.BoolProperty: np.bool,
    bpy.types.IntProperty: np.int32,
    bpy.types.FloatProperty: np.float32,
    bpy.types.EnumProperty: np.int32,
})


def column[T: Struct](items: Elements[T], kind: type[T], member: str, /) -> Buffer:
    rna: bpy.types.Struct = kind.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    match rna.properties[member]:
        case bpy.types.BoolProperty(array_dimensions=dims) | bpy.types.IntProperty(array_dimensions=dims) | bpy.types.FloatProperty(array_dimensions=dims) as described:
            buffer: Buffer = np.empty((len(items), *np.trim_zeros(np.asarray(dims))), STORED[type(described)])
        case described:
            buffer = np.empty(len(items), STORED[type(described)])
    items.foreach_get(member, buffer.reshape(-1))
    return buffer
```

`column(scene.objects, bpy.types.Object, "matrix_world")` returns `(objects, 4, 4)` in stored column order, `column(scene.objects, bpy.types.Object, "type")` each object's type `value`, and a string or pointer member raises `KeyError`.

Buffers follow host storage:
- Topology sits in internal attributes: `.corner_vert` and `.corner_edge` key `value` per corner, `.edge_verts` key `value` 2 wide per edge, and face offsets in `polygons` key `loop_start`
- Dynamic arrays (`MeshPolygon.vertices`) declare one fixed `array_length` while each item holds its own, and face vertices read through `.corner_vert` with `loop_start`
- `INT8` attributes store `int8`, an `int8` buffer copying them whole
- `BYTE_COLOR` stores 8-bit sRGB and `FLOAT_COLOR` linear floats, `color` reading linear and `color_srgb` sRGB from both, so a copy of stored values reads the member of the layer's own space
- `Image.pixels.foreach_get(buffer)` takes `float32`, channels per pixel, bottom row first
- Meshes with shape keys evaluate positions from `key_blocks`, a `position` write reaching neither the evaluated mesh nor the reference key, and keys write through `ShapeKey.points` key `co`, each key's `value` mixing it over its `relative_key`
- Curve shape keys hold an empty `points` and a `data` mixing `ShapeKeyBezierPoint` and `ShapeKeyCurvePoint` items under `fixed_type` `UnknownType`
- Hair `curve_offset_data` and Grease Pencil `drawing.curve_offsets` hold curve offsets in `value`, one more than the curve count
- Legacy `Curve` splines hold no attributes, `splines.new` starting a spline with one point that `bezier_points.add(n)` and `points.add(n)` extend, and `SplinePoint.co` holding 4 values with the weight last
- `foreach_set` on attribute data tags the mesh for evaluation

Exporters transform the temporary evaluated mesh in place through `Mesh.transform` and copy its buffers out before `to_mesh_clear`:

```python
def surface(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, /) -> Option[tuple[Buffer, Buffer, Buffer]]:
    evaluated: bpy.types.Object = source.evaluated_get(depsgraph)

    def read(mesh: bpy.types.Mesh) -> tuple[Buffer, Buffer, Buffer]:
        positions: bpy.types.FloatVectorAttribute = mesh.attributes["position"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        corners: bpy.types.IntAttribute = mesh.attributes[".corner_vert"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        mesh.transform(evaluated.matrix_world)
        return (
            column(positions.data, bpy.types.FloatVectorAttributeValue, "vector"),
            column(corners.data, bpy.types.IntAttributeValue, "value"),
            column(mesh.polygons, bpy.types.MeshPolygon, "loop_start"),
        )

    with ExitStack() as scope:
        scope.callback(evaluated.to_mesh_clear)
        return Option.of_optional(evaluated.to_mesh()).map(read)
```

`to_mesh()` returns `None` for geometry evaluating to no mesh (an empty curve) and raises `RuntimeError` on a camera, empty, or point cloud.

Construction sizes each domain from its buffer for every `foreach_set` length to hold, and `update()` derives edges before `validate()` judges topology:

```python
def built(name: str, positions: NDArray[np.float32], corners: NDArray[np.int32], starts: NDArray[np.int32], /) -> Result[bpy.types.Mesh, bpy.types.Mesh]:
    mesh: bpy.types.Mesh = bpy.data.meshes.new(name)
    mesh.vertices.add(len(positions))
    mesh.loops.add(len(corners))
    mesh.polygons.add(len(starts))
    placed: bpy.types.FloatVectorAttribute = mesh.attributes["position"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    cornered: bpy.types.IntAttribute = mesh.attributes[".corner_vert"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    placed.data.foreach_set("vector", positions.reshape(-1))
    cornered.data.foreach_set("value", corners)
    mesh.polygons.foreach_set("loop_start", starts)
    mesh.update()
    return Ok(mesh).filter_with(lambda held: not held.validate(), identity)
```

`validate()` returns `True` when it repaired the mesh, dropping faces with an out-of-range corner or offset, and the `Error` holds the repaired mesh for a `WARNING` report. `validate()` before `update()` reads missing edges as damage and returns `True` on valid input.

## [05]-[ANIMATION]

Channels follow the layered action model:
- `action.fcurve_ensure_for_datablock(datablock, data_path, index=, group_name=)` on an action assigned to `datablock` creates its layer, strip, slot, and channelbag and returns an existing F-curve, an unassigned action raising `RuntimeError`
- Actions assigned to a second ID leave `animation_data.action_slot` `None` with `action_suitable_slots` listing the slot, and `fcurve_ensure_for_datablock` adds a slot for that ID, IDs sharing one slot assigning `action_slot` first
- Actions hold one layer, a second `layers.new` raising `RuntimeError`, and stacked animation runs through NLA tracks whose strip `blend_type` composes each channel, `COMBINE` taking a scale of 2 under 3 to 6 where `ADD` takes it to 5
- `keyframe_insert` and `keyframe_points.add` create `BEZIER` keys whatever the interpolation preference
- `fcurve.update()` sorts keys and computes their handles, and `co_ui` moves a key with its handles where `co` moves the key alone

Keys replace a channel's keys through one `keyframe_points.add(count)`, `foreach_set` on `co` and `interpolation`, then `fcurve.update()`:

```python
def keyed(action: bpy.types.Action, owner: bpy.types.ID, path: str, points: NDArray[np.float32], /, *, index: int = 0, interpolation: str = "LINEAR") -> Result[bpy.types.FCurve, RuntimeError]:
    modes: bpy.types.EnumProperty = bpy.types.Keyframe.bl_rna.properties["interpolation"]  # type: ignore[assignment]  # ty: ignore[invalid-assignment]

    def filled(curve: bpy.types.FCurve) -> bpy.types.FCurve:
        curve.keyframe_points.clear()
        curve.keyframe_points.add(len(points))
        curve.keyframe_points.foreach_set("co", points.reshape(-1))
        curve.keyframe_points.foreach_set("interpolation", np.full(len(points), modes.enum_items[interpolation].value, np.int32))
        curve.update()
        return curve

    return catch(action.fcurve_ensure_for_datablock, exception=RuntimeError)(owner, path, index=index).map(filled)
```

`keyed(action, light, "energy", points)` keys a light's energy from `(frames, 2)` frame and value pairs, and an action not assigned to `light` returns the `RuntimeError`.

## [06]-[ARMATURES]

Bones hold one representation per mode:
- `arm.edit_bones` holds `EditBone` rest data in Edit Mode alone, `edit_bones.new` outside it raising `RuntimeError`
- `arm.bones` holds `Bone` rest data at its last Object Mode state, and `obj.pose.bones` gains each `PoseBone` at Edit Mode exit
- `PoseBone.matrix` reads armature space, `matrix_basis` the local channels, and `Bone.matrix_local` the rest, `obj.convert_space(pose_bone=, matrix=, from_space=, to_space=)` moving a matrix between `WORLD`, `POSE`, `LOCAL_WITH_PARENT`, and `LOCAL`

Bones build in one Edit Mode scope that returns their names:

```python
def chained(layer: bpy.types.ViewLayer, rig: bpy.types.Object, joints: Sequence[Vector], /) -> tuple[str, ...]:
    armature: bpy.types.Armature = rig.data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    layer.objects.active = rig
    with ExitStack() as scope:
        bpy.ops.object.mode_set(mode="EDIT")
        scope.callback(bpy.ops.object.mode_set, mode="OBJECT")
        made = tuple(armature.edit_bones.new(rig.name) for _ in joints[1:])
        for bone, parent, (head, tail) in zip(made, (None, *made[:-1]), pairwise(joints), strict=True):
            bone.head, bone.tail, bone.parent, bone.use_connect = head, tail, parent, parent is not None
        return tuple(bone.name for bone in made)
```

`chained(context.view_layer, rig, joints)` builds four joints into three connected bones, each parented to the one before, and `rig.pose.bones[name]` reads each pose bone after the call.
