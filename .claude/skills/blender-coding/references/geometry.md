# [GEOMETRY]

## [01]-[EVALUATED_READS]

Evaluated geometry reads through the member working in the space a task needs:
- `obj.ray_cast(origin, direction, distance=, depsgraph=)` and `obj.closest_point_on_mesh(origin, distance=, depsgraph=)` work in object space
- `obj.ray_cast` and `closest_point_on_mesh` return face indices into evaluated mesh
- `scene.ray_cast(depsgraph, origin, direction, distance=)` works in world space and returns the hit object with its matrix
- `bm.from_object(obj, depsgraph, cage=, face_normals=, vertex_normals=)` fills a BMesh from the evaluated mesh
- `GeometrySet` holds `mesh`, `mesh_base` (pre-subdivision mesh), `curves`, `pointcloud`, `volume`, and `grease_pencil`
- `GeometrySet.from_evaluated_object` matches `evaluated_geometry()`
- `instances_pointcloud()` holds a point per instance, `instance_transform` column-major with translation at `[3, :3]`
- Instance point `.reference_index` indexes `instance_references()` (`Object`, `Collection`, `GeometrySet`, or `None`)
- `depsgraph.object_instances` takes `len` and iteration with no index, `persistent_id` holding eight integers, zeros outside an instance

## [02]-[BMESH]

BMesh ownership follows the mode:
- Outside Edit Mode, `bmesh.new()` registers `free` on scope `ExitStack` and writes through `bm.to_mesh(mesh)`, an Edit Mode mesh raising `ValueError`
- In Edit Mode, `bmesh.from_edit_mesh(mesh)` borrows live mesh, held BMesh and element references raising `ReferenceError` after mode change or undo
- In Edit Mode, `mesh.attributes` holds no `position`, and a `position` attribute held from Object Mode reads `name` `""` with no raise
- `bmesh.update_edit_mesh(mesh)` follows each change
- Selection-only changes call `bm.select_flush_mode()` first and pass `loop_triangles=False, destructive=False` to `update_edit_mesh`
- Edit Mode enters one object per shared mesh, others reading `mode` `OBJECT` over an `is_editmode` mesh
- Multi-object Edit Mode work iterates `objects_in_mode_unique_data`
- Index reads follow `ensure_lookup_table()` on each new or borrowed BMesh and topology edit (`bmesh.ops.create_cube` included)
- Stale lookup tables raise `IndexError`
- New elements join live iterations and hold index -1 until `index_update()`
- Face and vertex normals stay stale through `co` writes until `bm.normal_update()`, and `to_mesh` derives mesh normals itself
- UV selection is `BMLoop.uv_select_vert`, `BMLoop.uv_select_edge`, and `BMFace.uv_select`
- `calc_uvs=True` on `bmesh.ops.create_*` writes into an existing UV layer alone, `bm.loops.layers.uv.verify()` creating it first

## [03]-[ATTRIBUTES]

Per-element geometry data sits in attributes, each built-in layer existing once its elements or values do:
- `vertices.add` creates `position`, `loops.add` creates `.corner_vert` and `.corner_edge`, and `update()` derives `.edge_verts` with the edges
- `Curves.add_curves(sizes)` and Grease Pencil `drawing.add_strokes(sizes)` append a curve per point count, `PointCloud.resize(count)` sizing a cloud
- Added curves and resized clouds hold `position` alone, Grease Pencil strokes adding `curve_type`
- `attributes.new(name, type, domain)` on a taken name, built-ins included, returns a `.001` layer
- Attribute writers test `name in attributes`, `data_type`, and `domain` first
- `uv_layers.new` creates a `FLOAT2` `CORNER` attribute
- Object Mode selection is `.select_vert`, `.select_edge`, and `.select_poly` `BOOLEAN` attributes
- Each mesh's first color attribute becomes `active_color_name` (painted) and `default_color_name` (rendered)
- `active_color` writes leave `default_color_name` unchanged
- `sharp_face`, `sharp_edge`, and `material_index` exist once written
- `shade_smooth()` removes `sharp_face`, and `set_sharp_from_angle(angle=)` writes `sharp_edge`
- Sculpt masks and face sets are `.sculpt_mask` and `.sculpt_face_set`
- Bevel weights and creases are `bevel_weight_vert`, `bevel_weight_edge`, `crease_vert`, and `crease_edge`
- `normals_domain` names the domain of complete normals, `corner_normals` reading per corner and `normals_split_custom_set` writing custom ones
- `loop_triangles` reads ensure tessellation, `vertices` holding 3 `int32` per triangle and `loop_triangle_polygons.value` its face
- Vertex groups sit outside attributes, weights reading through `MeshVertex.groups` and names stored on the mesh for every sharing object
- `VertexGroup.remove` takes indices below `len(mesh.vertices)`, a larger index crashing Blender

## [04]-[BUFFERS]

Collection `foreach_get` and `foreach_set` take a flat sequence of `len(collection)` times member width. RNA raw access copies matching types as one buffer for contiguous storage and per item for strided members, converting other values per element. Dtype and shape derive from item-class RNA, enum members reading each item's `value`:

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

`column(scene.objects, bpy.types.Object, "matrix_world")` returns `(objects, 4, 4)` in stored column order. `column(scene.objects, bpy.types.Object, "type")` returns each type `value`, and string or pointer members raise `KeyError`.

Property-array bulk access supports int/float storage with matching buffer format and item width (`float32` for floats); sequences convert per element. `foreach_set` omits RNA updates; native index and slice writes enforce editability and run updates. Raw matrix nesting follows RNA storage order, unlike Mathutils row indexing.

Buffers follow host storage:
- Topology is `.corner_vert` and `.corner_edge` key `value` per corner, `.edge_verts` key `value` 2 per edge, `polygons` key `loop_start` face offsets
- Dynamic arrays (`MeshPolygon.vertices`) declare a fixed `array_length` though items vary, face vertices reading `.corner_vert` with `loop_start`
- `INT8` attributes store `int8`, an `int8` buffer copying them whole
- `BYTE_COLOR` stores 8-bit sRGB and `FLOAT_COLOR` linear floats, `color` reading linear and `color_srgb` sRGB from both
- Stored-value copies of color attributes read each layer's own space
- `Image.pixels.foreach_get(buffer)` takes `float32`, channels per pixel, bottom row first
- Meshes with shape keys evaluate positions from `key_blocks`, a `position` write reaching neither the evaluated mesh nor the reference key
- Shape keys write through `ShapeKey.points` key `co`, each key's `value` mixing it over its `relative_key`
- Curve shape keys hold an empty `points` and a `data` mixing `ShapeKeyBezierPoint` and `ShapeKeyCurvePoint` items under `fixed_type` `UnknownType`
- Hair `curve_offset_data` and Grease Pencil `drawing.curve_offsets` hold curve offsets in `value`, one more than the curve count
- `Curve` splines hold no attributes, `splines.new` starting one point that `bezier_points.add(n)` and `points.add(n)` extend
- `SplinePoint.co` holds 4 values, weight last
- `foreach_set` on attribute data tags the mesh for evaluation

Exporters transform the temporary evaluated mesh in place through `Mesh.transform` and copy its buffers out before `to_mesh_clear`:

```python
type Surface = tuple[Buffer, Buffer, Buffer]


def surface(source: bpy.types.Object, depsgraph: bpy.types.Depsgraph, /) -> Option[Surface]:
    evaluated: bpy.types.Object = source.evaluated_get(depsgraph)

    def read(mesh: bpy.types.Mesh) -> Surface:
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

Construction sizes each domain from its topology buffers, writes their values, and lets `update()` derive edges:

```python
def built(name: str, positions: NDArray[np.float32], corners: NDArray[np.int32], starts: NDArray[np.int32], /) -> bpy.types.Mesh:
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
    return mesh
```

## [05]-[ANIMATION]

Channels follow the layered action model:
- `fcurve_ensure_for_datablock(datablock, data_path, index=, group_name=)` creates layer, strip, slot, and channelbag or returns existing F-curve
- `fcurve_ensure_for_datablock` on an unassigned action raises `RuntimeError`
- Actions assigned to a second ID leave its `action_slot` `None` (`action_suitable_slots` lists the slot)
- `fcurve_ensure_for_datablock` adds a slot unless `action_slot` is set
- Actions hold one layer, a second `layers.new` raising `RuntimeError`
- Stacked animation runs through NLA tracks, strip `blend_type` composing each channel (`COMBINE` takes scale 2 under 3 to 6, `ADD` to 5)
- `keyframe_insert` and `keyframe_points.add` create `BEZIER` keys whatever the interpolation preference
- `fcurve.update()` sorts keys and computes their handles, and `co_ui` moves a key with its handles where `co` moves the key alone

Keys replace a channel's keys through one `keyframe_points.add(count)`, `foreach_set` on `co` and `interpolation`, then `fcurve.update()`:

```python
def keyed(action: bpy.types.Action, owner: bpy.types.ID, path: str, points: NDArray[np.float32], interpolation: "rna_enums.BeztripleInterpolationModeItems", /, *, index: int = 0) -> Result[bpy.types.FCurve, RuntimeError]:
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

`keyed(action, light, "energy", points, context.preferences.edit.keyframe_new_interpolation_type)` keys light energy from `(frames, 2)` frame and value pairs at the user's interpolation, returning `RuntimeError` for an action unassigned to `light`.

## [06]-[ARMATURES]

Bones hold one representation per mode:
- `arm.edit_bones` holds `EditBone` rest data in Edit Mode alone, `edit_bones.new` outside it raising `RuntimeError`
- `arm.bones` holds `Bone` rest data at its last Object Mode state, and `obj.pose.bones` gains each `PoseBone` at Edit Mode exit
- `PoseBone.matrix` reads armature space, `matrix_basis` local channels, and `Bone.matrix_local` rest
- `obj.convert_space(pose_bone=, matrix=, from_space=, to_space=)` converts between `WORLD`, `POSE`, `LOCAL_WITH_PARENT`, and `LOCAL`

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

`chained(context.view_layer, rig, joints)` connects a bone between each consecutive joint pair, parented to the one before. `rig.pose.bones[name]` reads each pose bone after the call.
