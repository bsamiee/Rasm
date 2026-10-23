# [GEOMETRY]

Covers evaluated reads, BMesh, mesh attributes, buffer paths and costs, and animation channels.

## [01]-[EVALUATED_READS]

Evaluated reads go through `blender.bpy.data.ids.evaluation` and follow host evaluation rules:
- `evaluated(view_layer)` updates the layer and returns its depsgraph, or an edit fault while evaluation runs
- `hit(obj, depsgraph, Ray(...) | Near(...))` runs an object-space ray cast or nearest-surface query and returns `BlenderResult[Option[Hit]]`, `Hit.face` indexing the evaluated mesh
- `Scene.ray_cast(depsgraph, origin, direction)` works in world space
- `BMesh.from_object(obj, depsgraph)` and `BVHTree.FromObject(obj, depsgraph)` read the evaluated mesh
- `obj.evaluated_get(depsgraph).evaluated_geometry()` answers a `GeometrySet` with `mesh`, `curves`, `pointcloud`, `volume`, `grease_pencil`, `instances_pointcloud()`, and `instance_references()`
- `instances_pointcloud()` holds one point per instance with `instance_transform` and `.reference_index`
- `instance_references()` lists `Object`, `Collection`, `GeometrySet`, or `None` per index
- `depsgraph.object_instances` takes `len` and no index, and `persistent_id` holds eight integers, zeros outside an instance

## [02]-[BMESH]

BMesh ownership follows the mode:
- Outside Edit Mode, `bmesh.new()` registers `free` on the scope's `ExitStack`, fills, and writes back through `to_mesh(mesh)`
- In Edit Mode, `bmesh.from_edit_mesh(mesh)` borrows the live mesh until a mode change or an Edit Mode undo
- Held BMesh and element references raise `ReferenceError` after that change
- In Edit Mode, `mesh.attributes` lists the edit mesh's layers with no `position`, a `position` attribute held from Object Mode reading `name` `""` with no raise
- `mesh.vertices` reads the pre-edit state until `obj.update_from_editmode()`, and `mesh.vertices[i].co` writes in Edit Mode are discarded at mode exit
- In Edit Mode, `bmesh.update_edit_mesh(mesh)` follows each change, a selection-only change calling `bm.select_flush_mode()` first and passing `loop_triangles=False, destructive=False`
- Multi-object Edit Mode work iterates `context.objects_in_mode_unique_data`, one entry per shared mesh
- Index reads follow `ensure_lookup_table()` on every new or borrowed BMesh and after every topology edit, `bmesh.ops.create_cube` included
- Loops that add elements iterate a copy (`list(bm.faces)`), new elements joining a live iteration with no raise
- Held elements a `bmesh.ops` call removes raise `ReferenceError` on access
- New elements hold index -1 until `index_update()`
- `bm.normal_update()` follows manual vertex moves before BMesh normals are read, `to_mesh` deriving mesh normals itself
- UV selection is `BMLoop.uv_select_vert`, `BMLoop.uv_select_edge`, and `BMFace.uv_select`
- `calc_uvs=True` on `bmesh.ops.create_*` writes into an existing UV layer alone, and `bm.loops.layers.uv.verify()` creates it first

## [03]-[MESH_ATTRIBUTES]

Per-element mesh data goes through attributes and dedicated calls:
- Normals read through `Mesh.corner_normals`, custom normals write through `mesh.normals_split_custom_set`, and `MeshLoop.normal` stays read-only
- `mesh.shade_smooth()` then `mesh.set_sharp_from_angle(angle=...)` smooths through the data API
- Sculpt mask is the `.sculpt_mask` point attribute, face sets `.sculpt_face_set`
- Bevel weights and creases are the `bevel_weight_edge` and `crease_edge` attributes
- `mesh.validate()` follows input not known to be valid and answers `True` when it repaired something
- `VertexGroup.remove` takes indices below `len(mesh.vertices)`, a larger index crashing Blender
- Object Mode `mesh.attributes` lists `position`, `.edge_verts`, `.corner_vert`, `.corner_edge`, `sharp_face`, UV maps, and `material_index`
- Vertex groups sit outside original and evaluated mesh attributes, weights reading through `MeshVertex.groups` alone
- Vertex group names belong to the mesh, every object sharing it listing them, and `vertex_groups.clear()` empties `groups` on each vertex

## [04]-[MESH_BUFFERS]

`foreach_get` and `foreach_set` take flat C-contiguous numpy buffers of the member's stored type, sized to `len(collection) * item_size`:

| [INDEX] | [NEED]                   | [MEMBER_AND_KEY]                                          | [BUFFER]                                        |
| :-----: | :----------------------- | :-------------------------------------------------------- | :---------------------------------------------- |
|  [01]   | Vertex positions         | `attributes["position"].data` key `vector`                | `float32`, 3 per vertex                         |
|  [02]   | Corner vertices          | `attributes[".corner_vert"].data` key `value`             | `int32` per corner                              |
|  [03]   | Corner edges             | `attributes[".corner_edge"].data` key `value`             | `int32` per corner                              |
|  [04]   | Edge vertices            | `attributes[".edge_verts"].data` key `value`              | `int32`, 2 per edge                             |
|  [05]   | Face corner offsets      | `polygons` key `loop_start`                               | `int32` per face                                |
|  [06]   | `FLOAT` attribute        | Key `value`                                               | `float32`                                       |
|  [07]   | `FLOAT_VECTOR` attribute | Key `vector`                                              | `float32`, 3 per item                           |
|  [08]   | `FLOAT_COLOR` attribute  | Key `color` linear, `color_srgb` encoded                  | `float32`, 4 per item                           |
|  [09]   | `BYTE_COLOR` attribute   | Key `color_srgb` keeping stored bytes, `color` converting | `float32`, 4 per item                           |
|  [10]   | `INT` attribute          | Key `value`                                               | `int32`                                         |
|  [11]   | `BOOLEAN` attribute      | Key `value`                                               | `bool`                                          |
|  [12]   | Image pixels             | `Image.pixels` through `foreach_get`                      | `float32`, channels per pixel, bottom row first |
|  [13]   | Keyframes                | `keyframe_points` key `co`                                | `float32`, 2 per key                            |
|  [14]   | Enum members             | `interpolation` and other enum keys                       | `enum_items[identifier].value` per item         |
|  [15]   | ID-collection members    | `bpy.data.<collection>` key `location`, `hide_viewport`   | Member's stored type                            |
|  [16]   | `mathutils` value        | `np.asarray(value, np.float32)`                           | `np.ascontiguousarray` before a handoff         |

Buffers follow host storage:
- Buffers are flat, and `reshape(-1, 3)` follows the read
- 2D buffers and wrong lengths raise `RuntimeError("internal error setting the array")`, the exception a `catch` names, and print `Array length mismatch`
- Dynamic arrays (`MeshPolygon.vertices`) declare one length in `array_length` and `array_dimensions` while each item holds its own, and buffers take the resolved array's `len()`
- Mesh shape keys read `co` through `ShapeKey.points`, 100k points in 25 µs against 3.5 ms through `data`
- Curve shape keys hold an empty `points`, and `data` mixes `ShapeKeyBezierPoint` and `ShapeKeyCurvePoint` items, `co` 3 wide on both, with `fixed_type` reading `UnknownType`
- Hair `curve_offset_data` and Grease Pencil `drawing.curve_offsets` hold curve offsets in `value`, one more than the curve count, outside the attributes
- `foreach_get` reads read-only members (`GreasePencilFrame.frame_number`)
- `foreach_set` on attribute data tags the mesh for evaluation
- `.corner_vert`, `.corner_edge`, and `.edge_verts` are internal attributes with no public fast accessor, and Blender's bundled FBX and glTF exporters read and write them as their bulk path

Each path falls in a cost class, ordered cheapest first:

| [INDEX] | [CLASS]               | [PATHS]                                                                                                          |
| :-----: | :-------------------- | :--------------------------------------------------------------------------------------------------------------- |
|  [01]   | Memcpy                | Attribute data with a flat buffer of the stored type, `Image.pixels.foreach_get`                                 |
|  [02]   | Per element           | Other `foreach_get` paths (`vertices` `co`, `polygons` `loop_start`), memcpy paths given another dtype or layout |
|  [03]   | Per element in Python | `v.co.x` loops, `pixels[:]`                                                                                      |

Hot code reads attribute data with the stored dtype:
- `float64`, `uintc`, `int64`, list, and strided buffers convert per element
- `polygons` `loop_start` is the one face-offset path

Exporters read world positions and topology from the evaluated mesh inside the `meshed` scope:

```python
def surface(source: bpy.types.Object, view_layer: bpy.types.ViewLayer) -> BlenderResult[tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.int32]]]:
    def read(mesh: bpy.types.Mesh) -> tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.int32]]:
        positions, corners, starts = np.empty(len(mesh.vertices) * 3, np.float32), np.empty(len(mesh.loops), np.int32), np.empty(len(mesh.polygons), np.int32)
        for key, member, buffer in (("position", "vector", positions), (".corner_vert", "value", corners)):
            if isinstance(attribute := mesh.attributes[key], bpy.types.FloatVectorAttribute | bpy.types.IntAttribute):
                attribute.data.foreach_get(member, buffer)
        mesh.polygons.foreach_get("loop_start", starts)
        world: NDArray[np.float32] = np.asarray(source.matrix_world, np.float32)
        return positions.reshape(-1, 3) @ world[:3, :3].T + world[:3, 3], corners, starts

    def scoped(depsgraph: bpy.types.Depsgraph) -> BlenderResult[tuple[NDArray[np.float32], NDArray[np.int32], NDArray[np.int32]]]:
        with meshed(source, depsgraph) as mesh:
            return mesh.map(read)

    return evaluated(view_layer).bind(scoped)
```

Construction sizes the mesh from the buffers, writes them flat, then derives edges, face sizes, and normals through `mesh.update()`:

```python
def built(name: str, positions: NDArray[np.float32], corners: NDArray[np.int32], starts: NDArray[np.int32]) -> BlenderResult[bpy.types.Mesh]:
    def filled(mesh: bpy.types.Mesh) -> bpy.types.Mesh:
        mesh.vertices.add(len(positions))
        mesh.loops.add(len(corners))
        mesh.polygons.add(len(starts))
        for key, member, buffer in (("position", "vector", positions.ravel()), (".corner_vert", "value", corners)):
            if isinstance(attribute := mesh.attributes[key], bpy.types.FloatVectorAttribute | bpy.types.IntAttribute):
                attribute.data.foreach_set(member, buffer)
        mesh.polygons.foreach_set("loop_start", starts)
        mesh.update()
        return mesh

    with ExitStack() as rollback:
        mesh: bpy.types.Mesh = bpy.data.meshes.new(name)
        rollback.callback(bpy.data.batch_remove, ids=(mesh,))
        return committed(catch(filled, exception=RuntimeError)(mesh).map_error(lambda error: (BlenderEditFault(request=name, error=error),)), rollback)
```

`positions` buffers not shaped `(n, 3)` raise `RuntimeError`, and `rollback` removes the mesh.

Bulk removal names objects and the data no other user holds in one `batch_remove`:

```python
def purged(objects: tuple[bpy.types.Object, ...]) -> None:
    held: set[bpy.types.ID] = set(objects)
    users: dict[bpy.types.ID, set[bpy.types.ID]] = bpy.data.user_map(subset={obj.data for obj in objects if obj.data is not None})
    owned: tuple[bpy.types.ID, ...] = tuple(block for block, holders in users.items() if holders <= held)
    bpy.data.batch_remove(ids=(*objects, *owned))
```

## [05]-[ANIMATION]

`blender.bpy.data.rna.animation` owns channels:
- `Animation(struct, member, index, source, modifiers).apply(owner, context)` reconciles one channel, keeps equal native state and unrelated channels, and answers whether it wrote
- `Keys(points=...)` writes keyframes, `Driven(expression=..., variables=...)` a scripted driver over the animated ID's own properties
- `Animation.clear(owner)` removes the declared channel
- `sampled(owner, path, frames)` evaluates the keyed F-curve per frame within the hard range without moving the scene, an unkeyed channel repeating its value
- Values a driver or NLA strip changes read through `frame_set`

```python
def keyed(light: bpy.types.Light, context: bpy.types.Context, points: tuple[tuple[float, float], ...]) -> BlenderResult[bool]:
    return Animation(member="energy", source=Keys(points=points)).apply(light, context)
```

Channel work `Animation` lacks joins `Animation` and follows the layered action model:
- F-curves sit in channelbags from `bpy_extras.anim_utils.action_ensure_channelbag_for_slot(action, slot)`
- `action_get_channelbag_for_slot` returns `None` before a channelbag exists
- Actions take one layer, and layered animation runs through NLA strips with `ADD` or `COMBINE` blending
- Actions assigned to a second ID animate nothing until `animation_data.action_slot` names their slot
- Keys write through one `keyframe_points.add(count)`, `foreach_set` on `co` and `interpolation`, then `fcurve.update()`
- Scripted `keyframe_insert` keys are `BEZIER` with `AUTO_CLAMPED` handles whatever the preference
- `co_ui` moves a key with its handles where `co` moves the key alone
- Driver F-curves from Python hold keys at (0, 0) and (1, 1)
- Moving a driver F-curve key remaps the driver value, and removing every key passes the raw value
