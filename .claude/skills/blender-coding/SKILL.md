---
name: blender-coding
description: "Use when writing or reviewing Python that runs inside Blender, covering bpy code from data references to host boundaries"
---

# [BLENDER_CODING]

Covers `bpy` code from holding a data reference to translating a result at a host entry point.

- Use `python-coding` for result types, records, pattern matching, and comprehensions

[REFERENCES]:
- [01]-[DATA](references/data.md): Storage owner per value lifetime, created IDs, library loads, paths, custom property values
- [02]-[OPERATORS](references/operators.md): `bpy.types.Operator` classes, from conditions and invoke forms to modal steps and placements
- [03]-[GEOMETRY](references/geometry.md): Evaluated reads, BMesh, mesh attributes, buffers derived from RNA, and animation channels
- [04]-[MATHUTILS](references/mathutils.md): `mathutils` values against numpy arrays, transforms over many objects, rotations, and spatial queries
- [05]-[LIFECYCLE](references/lifecycle.md): Callback bodies across file loads, updates, deferred writes, frames, drivers, and worker processes
- [06]-[TYPING](references/typing.md): Annotation forms under eager evaluation, stub declarations runtime contradicts, and the code form each takes

## [01]-[DATA]

Host references go stale after their event, and code holds the form that survives:

| [INDEX] | [REFERENCE]                          | [STALE_AFTER]                                 | [HOLD]                                       |
| :-----: | :----------------------------------- | :-------------------------------------------- | :------------------------------------------- |
|  [01]   | ID                                   | `remove()`, undo step touching it, file load  | `session_uid`, name and library after a load |
|  [02]   | Vertex, bone, point, UV, attribute   | `mode_set`, container growth, ID removal      | Index or name, re-read after the change      |
|  [03]   | `CollectionProperty` item            | `add()` on its collection                     | Index, re-read after each `add()`            |
|  [04]   | Grease Pencil stroke or point        | `remove_strokes`, `resize_strokes`            | `frame.drawing` read again                   |
|  [05]   | `Keyframe`                           | Next insert on its F-curve                    | `keyframe_points` re-read per insert         |
|  [06]   | `mathutils` value read from RNA      | Owner write (read live), owner removal        | `.copy()`                                    |
|  [07]   | `PropertyGroup` on an ID             | Undo                                          | Re-resolved through its ID                   |
|  [08]   | Evaluated ID                         | `Depsgraph.update()`, rewriting it in place   | Values copied out per evaluation             |
|  [09]   | `object_instances` item              | Next iteration step                           | Values copied per step                       |
|  [10]   | `to_mesh()` result                   | Re-evaluation, `to_mesh()`, `to_mesh_clear()` | Scope calling `to_mesh_clear()`              |
|  [11]   | GeometrySet component                | Release of its GeometrySet                    | GeometrySet bound for the read               |
|  [12]   | Struct with a class a write refines  | `Light.type`, interface `socket_type`         | Re-read from its owner after the write       |
|  [13]   | Generated `properties.inputs` struct | Group assignment, any interface write         | `properties.inputs` re-read after each write |
|  [14]   | Pointer an operator re-targets       | `object.convert`, importer `obj.data` swaps   | Read through its owner after the call        |
|  [15]   | `ui_styles` item                     | Theme reset, preset load                      | Re-read from its owner                       |
|  [16]   | Dynamic enum `EnumPropertyItem`      | End of `enum_items` iteration listing it      | Item `identifier`                            |

Stale references fail by kind:
- Stale ID, `object_instances` item, `to_mesh()` mesh, and GeometrySet component references raise `ReferenceError` on every read
- Stale sub-data, `Keyframe`, `PropertyGroup`, and `mathutils` references read freed or reused memory with no raise, and reads return wrong values or crash Blender
- Held generated input structs crash Blender at their next read after an interface write, a `default_value` write included

Writes to a governing member reset governed members:
- Unit `system` writes reset `length_unit` and `mass_unit`, an equal value included, and leave `temperature_unit` and `time_unit`
- `rotation_mode` names the rotation member evaluation reads, and writes to the other rotation members store and change nothing
- `parent` writes keep `matrix_basis` and move the object, and a `matrix_world` copied before the write and assigned after it keeps the placement

Creation, removal, selection, and scope follow host rules:
- `.new()` returns the ID or node to keep, a name taken in its collection or tree suffixed `.001`, an ID name cut at 255 bytes
- IDs from `bpy.data.<collection>.new` and `new_from_object` meshes start with zero users until code links them, and saves drop zero-user IDs
- `bpy.data.batch_remove(ids=...)` removes the IDs named alone, scanning users once where each `remove()` scans every user
- `batch_remove` crashes Blender on a removed ID, and held IDs resolve by `session_uid` before the call
- Objects a call acts on come from the context scope (`context.scene.objects`, `view_layer.objects`, a collection), `bpy.data.<collection>` holding other scenes, orphans, and linked data
- Current data comes from its owner (`scene.world`, `context.scene`), `bpy.data.<collection>[0]` naming the alphabetically first block
- `select_set(True)` raises `RuntimeError` for an object outside the view layer and changes nothing on a `hide_select` object
- Visibility has one owner per scope: `hide_set` per view layer, `hide_viewport` on objects and collections globally, `LayerCollection.exclude` and `hide_viewport` per layer, `visible_get()` combining them

Removing an object leaves its data at zero users, and removals name objects with the data no other user holds in one `batch_remove`:

```python
def purged(objects: tuple[bpy.types.Object, ...]) -> None:
    held: frozenset[bpy.types.ID] = frozenset(objects)
    users: dict[bpy.types.ID, set[bpy.types.ID]] = bpy.data.user_map(subset={obj.data for obj in objects if obj.data is not None})
    bpy.data.batch_remove(ids=(*objects, *(block for block, holders in users.items() if holders <= held)))
```

Identity and lookup follow host keys:
- `bpy.data.<collection>[(name, None)]` reads local data alone, `(name, <library path>)` linked data
- Name, index, `in`, and `get` lookups on `bpy.data` collections scan linearly, and operations resolving many build one `{session_uid: id}` map
- `bpy.data.all_ids` iterates every ID except embedded data (material, world, light, and scene node trees, each `Scene.collection`)
- `bpy.data.<collection>` iterates in name order, and loops that rename iterate `bpy.data.<collection>[:]`
- Loops writing object visibility iterate `Collection.all_objects[:]`
- Default names (`"Material Output"`) translate with the interface language, and code holds the returned reference or matches `type`
- Embedded node trees sit outside `bpy.data.node_groups` with `id_data` naming each tree, and keys over them take the holding ID
- Struct wrappers rebuild per access and ID wrappers per class refinement, and structs compare with `==`
- Caches key by `session_uid`, stable across renames, undo, and an undone removal while `as_pointer()` addresses repeat
- `bl_rna.identifier` names a refined subtype (`SunLight`), and dispatch matches base classes through `isinstance` or a class pattern
- Collection struct type tests (`ObjectModifiers`) never match a `bpy_prop_collection` value, and code binds the collection's own member
- RNA members of a class test through `name in T.bl_rna.properties` or `T.bl_rna.functions`
- Operators test through `"<name>" in dir(bpy.ops.<module>)` and list parameters through `get_rna_type().properties`, `hasattr` answering true for any name
- Dynamic enums (engine, color management, OCIO views) list a static placeholder in `enum_items` (`BLENDER_EEVEE`, `NONE`), and assignments outside the live items raise `TypeError` listing them
- `UILayout.enum_item_name(data, prop, identifier)` reads a dynamic enum's live label and `""` for an identifier outside the live items
- Collections can hold `None` items (`texture_slots`), and iteration enumerates before skipping `None` to keep host indices

Data paths and msgbus paths hold member, index, and `["<name>"]` key steps as a tuple, a tuple prefix answering containment, and render text once at the call taking it:

```python
type Step = str | int | tuple[str]


def spelled(step: Step) -> str:
    match step:
        case str():
            return f".{step}"
        case int():
            return f"[{step}]"
        case (key,):
            return f'["{bpy.utils.escape_identifier(key)}"]'


def path(*steps: Step) -> str:
    return "".join(map(spelled, steps)).removeprefix(".")
```

`path("render", "views", 0, "name")` resolves on a scene, and `obj.keyframe_insert(path(('<key>',)))` keys a custom property with quotes in its name. Generated expression text quotes values through `repr`.

## [02]-[OPERATORS]

Operators read context and act on the whole selection, and a per-object loop compounds edits and the costs each call pays:
- Context resolve
- View-layer update before the call and again after a `FINISHED` answer
- Undo bookkeeping

Work takes its data API form where one member does it, and a stock operator, `bpy_extras`, `_bpy_internal`, or bundled add-on member where Blender includes the procedure:

| [INDEX] | [WORK]                      | [FORM]                                                       |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | New ID                      | `bpy.data.<collection>.new`, then an explicit link           |
|  [02]   | Topology edit               | `bmesh` scope                                                |
|  [03]   | Per-element values          | `foreach_set` over attributes                                |
|  [04]   | Keys and drivers            | `keyframe_points.add` with `foreach_set`, `driver_add`       |
|  [05]   | Active and selected objects | `view_layer.objects.active`, `select_set`, set independently |
|  [06]   | Operator override           | Every member the operator reads, derived members included    |
|  [07]   | Procedure Blender includes  | Included member, operators under a `temp_override`           |

Overrides reach the members an operator reads alone:
- `selected_objects` overrides leave `selected_editable_objects` reading the window's own selection
- `object.mode_set` acts on `view_layer.objects.active` and exporters' selected-only option reads object selection whatever the override holds, both answering `FINISHED`
- Overrides nest a region inside its area inside its window, and take no full-screen area or temporary screen
- `window=` alone sets workspace and screen and clears area and region
- `screen=` of another workspace switches the window's workspace and the active object's mode for the block
- Areas outside the screen and regions outside the area raise `TypeError` at `__enter__`, and members passed as `None` enter with no raise
- `temp_override` accepts unknown keywords with no raise
- `logging_set(True)` called on the override before its `with` prints each member an operator reads with the calling line
- Background runs hold `window` and `screen` with no area, and an editor operator's override takes the `VIEW_3D` area from `window.screen.areas` with its `WINDOW` region

`object.join` reads `active_object` and `selected_editable_objects` and leaves the joined meshes at zero users:

```python
def merged(context: bpy.types.Context, target: bpy.types.Object, items: tuple[bpy.types.Object, ...]) -> Result[None, RuntimeError]:
    joined: tuple[bpy.types.ID, ...] = tuple(item.data for item in items if item.data is not None)
    with context.temp_override(active_object=target, selected_editable_objects=[target, *items]):
        answer = catch(bpy.ops.object.join, exception=RuntimeError)()
    return answer.map(lambda _: bpy.data.batch_remove(ids=tuple(block for block in joined if block.users == 0)))
```

Calls and their answers:
- Script calls fill unset properties from scene settings (`object.bake` from `scene.render.bake`) and user preferences (align, Edit Mode entry)
- Calls pass every value their result depends on
- `'INVOKE_DEFAULT'` runs `invoke`, and `wm.call_menu` crashes a background run under either call form
- Failing `poll` raises `RuntimeError("Operator bpy.ops.<op>.poll() ...")`
- `ERROR`, `ERROR_INVALID_INPUT`, `ERROR_INVALID_CONTEXT`, and `ERROR_OUT_OF_MEMORY` reports raise `RuntimeError` under either return set
- `INFO` and `WARNING` reports print
- `FINISHED` reports that a stock operator ran (`mesh.fill_grid` on an odd loop adds no face), and work depending on its effect reads the data it wrote
- Calls answer `{'FINISHED'}`, `{'RUNNING_MODAL'}`, `{'INTERFACE'}`, `{'CANCELLED'}`, or `{'PASS_THROUGH'}`, and code reads `CANCELLED` and `PASS_THROUGH` as a refusal
- Work behind `RUNNING_MODAL` or `INTERFACE` finishes after the call in a GUI session, dependent reads running from a later timer step
- Stock operators can answer `CANCELLED` with nothing to act on, and code tests the selection or data before the call
- `window.workspace` and file-browser `params` writes apply one event-loop pass later, dependent work running from a timer or modal step
- `area.type` and `show_region_*` writes apply inside the write

Created IDs are the `bpy.data.all_ids` difference across a call in place of a requested name or `selected_objects`, and a raising call keeps its writes, the created set leaving through one `batch_remove`:

```python
def imported(source: Path) -> Result[frozenset[bpy.types.ID], RuntimeError]:
    before = frozenset(bpy.data.all_ids)
    answer = catch(bpy.ops.wm.obj_import, exception=RuntimeError)(filepath=str(source))
    created = frozenset(bpy.data.all_ids) - before
    if answer.is_error():
        bpy.data.batch_remove(ids=tuple(created))
    return answer.map(lambda _: created)
```

## [03]-[EVALUATION]

Original data holds no modifier, constraint, or animation result, and each read or write takes its path:

| [INDEX] | [NEED]                                         | [PATH]                                                                                  |
| :-----: | :--------------------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Modified or animated state                     | `evaluated_get` on one `view_layer.depsgraph` per operation after `view_layer.update()` |
|  [02]   | Selection, identity, any write                 | Evaluated ID's `.original`                                                              |
|  [03]   | `matrix_world`, `dimensions`, `bound_box`      | `view_layer.update()` after data write                                                  |
|  [04]   | World extents                                  | `matrix_world` over evaluated geometry                                                  |
|  [05]   | Keyed F-curve value at other frames            | `fcurve.evaluate(frame)`                                                                |
|  [06]   | Evaluated state at another frame               | `frame_set` on window scene, frame restored after                                       |
|  [07]   | Instances                                      | `depsgraph.object_instances`                                                            |
|  [08]   | Evaluated mesh                                 | `evaluated_get(depsgraph).to_mesh()`                                                    |
|  [09]   | Evaluated attributes, instance references      | `obj.evaluated_get(depsgraph).evaluated_geometry()`                                     |
|  [10]   | Persistent evaluated mesh                      | `new_from_object(evaluated, preserve_all_data_layers=True, depsgraph=depsgraph)`        |
|  [11]   | Result after a write that tags nothing         | `id.update_tag()` on written ID, then evaluation                                        |
|  [12]   | Result after a modifier input write            | `obj.update_tag()`, then evaluation                                                     |
|  [13]   | Original mesh read in Edit Mode                | `obj.update_from_editmode()` first                                                      |
|  [14]   | `view_layer.objects` after a link or `exclude` | `view_layer.update()` first                                                             |

Evaluation follows host rules:
- Writes to an evaluated ID succeed with no raise and drop at the next evaluation
- Evaluated IDs hold a `session_uid` of their own, and keys read it from `.original`
- RNA writes to built-in members tag their owner, while `id["key"]` writes, script writes to registered `bpy.props`, `CurveProfile` point writes with `update()`, `PointCloud.resize`, and `GreasePencilFrames.move` tag nothing
- `scene.frame_current = n` applies at the next `view_layer.update()`, and `frame_set` evaluates at the call
- `view_layer.update()` and `view_layer.depsgraph` serve every scene's layers, and `context.evaluated_depsgraph_get()` returns the active layer's depsgraph
- `evaluated_depsgraph_get()` evaluates at viewport settings, and exports needing render results pass `evaluation_mode` (USD, Alembic, PLY, STL) or `export_eval_mode` (OBJ)
- `evaluated_geometry()` raises `TypeError` on an original or an object without geometry, and `instances_pointcloud()` reads `None` with no instances
- Instances stay out of the evaluated mesh and `dimensions`
- `Object.hide_viewport`, `Collection.hide_viewport`, and `LayerCollection.exclude` take objects out of the depsgraph, and `hide_set` objects still evaluate
- Evaluated meshes hold their source mesh's name, and code branches on the object it evaluated

## [04]-[PROPERTIES]

Settings, operator properties, and preferences are `bpy.props` annotations on `PropertyGroup`, `Operator`, and `AddonPreferences` classes, passing Blender's own keywords (`min`, `max`, `soft_min`, `soft_max`, `step`, `precision`, `subtype`) and a `use_<name>` `BoolProperty` beside each value it switches. Enum items copying an RNA enum derive from its `enum_items` as 5-tuples keeping each item's `value`, and update callbacks act on `self.id_data` in place of the active item:

```python
type Struct = bpy.types.bpy_struct[object]


def copied(owner: type[Struct], member: str) -> tuple[tuple[str, str, str, str, int], ...]:
    rna: bpy.types.Struct = owner.bl_rna  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    match rna.properties[member]:
        case bpy.types.EnumProperty(enum_items=items):
            return tuple((item.identifier, item.name, item.description, item.icon, item.value) for item in items)
        case other:
            raise TypeError(f"{rna.identifier}.{member} is {other.type}, not ENUM")


def retagged(self: bpy.types.PropertyGroup, _context: bpy.types.Context) -> None:
    owner: bpy.types.ID = self.id_data  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    owner.update_tag()


class Lamp(bpy.types.PropertyGroup):
    kind: bpy.props.EnumProperty(name="Kind", items=copied(bpy.types.Light, "type"), default="SUN", update=retagged)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_reach: bpy.props.BoolProperty(name="Reach", update=retagged)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    reach: bpy.props.FloatProperty(name="Reach", subtype="DISTANCE", min=0.0, soft_max=100.0, default=10.0, update=retagged)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
```

`kind` stores `AREA` as 4, its `Light.type` value, where a 3-tuple list stores its position 3.

`bpy.props` definitions take the form the host registers:
- `poll` and `update` take function objects with exact positional arity, defaulted parameters counted, and a factory returns the closure
- Bad arguments print a `TypeError` and drop the property at `register_class` with the class registered, integer bounds past `int32` printing `OverflowError`
- Float bounds past `float32` store as infinity, and `min > max` and out-of-range defaults register with no error
- Assignment clamps to hard bounds with no raise and leaves soft bounds unenforced
- Operator properties hold an ID by name or `session_uid`, an ID `PointerProperty` on an operator printing a `ValueError` at `register_class` and the operator registering without it
- `PointerProperty` takes ID and `PropertyGroup` types, `CurveMapping`, `ColorRamp`, `Scopes`, and `Histogram` coming from an owning ID
- `PointerProperty` `poll` filters UI assignment alone, and assigning an embedded ID (`Scene.collection`, `Material.node_tree`) raises `RuntimeError`
- `FloatProperty` and `FloatVectorProperty` take their unit from `subtype` (`DISTANCE` and `XYZ_LENGTH` read `unit` `LENGTH`), and a `unit` the subtype lacks logs an enum mismatch on each read of `subtype`
- `get` and `set` hold values stored outside Blender, `set` needing `get` and `get` alone making the property read-only
- `get_transform(self, stored_value, is_set)` and `set_transform(self, new_value, stored_raw_value, is_set)` transform stored values
- `set_transform` results clamp to the hard range, `get` and `get_transform` results reach readers unclamped, and a wrong type prints `TypeError`
- `update`, `get`, `set`, and transform callbacks can run in a threaded context per data-block, call no operator, and touch their own ID alone
- Dynamic `items` callbacks keep their strings referenced, answer a `None` context, and number items stably per identifier, a dynamic `EnumProperty` taking an integer default
- `ENUM_FLAG` items number by powers of 2
- `FILE_PATH` and `DIR_PATH` properties accepting `//` declare `PATH_SUPPORTS_BLEND_RELATIVE`
- Operator property update callbacks receive properties alone as `self`, with the window manager as `id_data`

Registered values read and write through the host:
- Values read and write as attributes and sit in system properties outside `ID.keys()`, `bl_system_properties_get()` listing them
- `ob["prop"] = value` on a registered name creates a separate custom property
- `property_unset` resets a value, and `is_property_set(name)` counts a value reused from the last run as unset, `ghost=False` counting it as set
- Walks over every writable member reach view state (`cursor_location`, `is_dirty`) and `PASSWORD`-subtype tokens (`auth_token`, `access_token`), and captures name the members they keep
- Caches over geometry key on `foreach_get` buffers, a member walk over a mesh paying per face
- `bl_rna.properties[name].is_readonly` is static, and `struct.is_property_readonly(name)` answers whether the member is editable now
- Data paths (`driver_add`, `keyframe_insert`, `path_resolve`) name a registered property by identifier, `["name"]` resolving ID properties alone
- `path_from_id()` raises `ValueError` for structs with no ID path (preferences, `Region`, `RegionView3D`, `CurveMapping`, `Stereo3dFormat`)
- `path_from_module()` returns a full path with the ID, and nested structs with no ID path join their member name to the holding struct's path

## [05]-[UNDO]

Undo restores `bpy.data` outside window manager and screen data, and each change pushes its step by form:

| [INDEX] | [CHANGE]                                 | [STEP]                                          | [FORM]                                     |
| :-----: | :--------------------------------------- | :---------------------------------------------- | :----------------------------------------- |
|  [01]   | Operator body that edits data            | One per `FINISHED` from UI, keymap, `undo` call | `bl_options` with `UNDO`                   |
|  [02]   | Script `bpy.ops` call                    | None, writes joining the next step              | `bpy.ops.<op>('EXEC_DEFAULT', True)`       |
|  [03]   | Operator called inside an `UNDO` body    | None, outer step holding every write            | Nested `bpy.ops` call                      |
|  [04]   | Window manager, screen, preference write | None                                            | `bl_options` without `UNDO`                |
|  [05]   | Steps the operator names itself          | One per `ed.undo_push(message=...)`             | `REGISTER` without `UNDO`                  |
|  [06]   | Repeated calls collapsing into one step  | One across consecutive calls of a group         | `UNDO_GROUPED`, no `UNDO`, `bl_undo_group` |
|  [07]   | Deferred callback write                  | One per write that changed data                 | Write, then `ed.undo_push(message=<step>)` |
|  [08]   | Background run                           | None until a script's `ed.undo_push()`          | `ExitStack` of restores and inverse writes |

Undo steps act on held state:
- Undo re-reads every ID its step touched at its old address, held references to touched IDs raising `ReferenceError` while untouched IDs keep working references
- `session_uid` lookups survive undo and reach the restored ID
- Undo restores registered groups, ID properties, and compositor inputs and runs no `update` callback
- Each undo push marks the file unsaved
- `window_manager.undo_stack` lists `steps`, `active_index`, and `active`, reading `None` before the first push

## [06]-[CALLBACKS]

Handler, timer, msgbus, and modal bodies run in a context Blender chose:
- Blender passes one argument to a handler function of one positional parameter and pads every other callable with `None`
- `load_*` and `save_*` handlers receive `""` as the startup file's path
- Depsgraph `*_post` handlers read the scene and depsgraph they receive
- `frame_set` fires `frame_change_pre` and `frame_change_post` alone
- `frame_change_pre` writes data for the frame, and `frame_change_post` handlers read evaluated results
- Timer functions return seconds until their next call, `None` stopping the timer
- Raising native timers unregister permanently, and a raising handler prints its traceback while later handlers run
- Handlers remove only themselves from their list during their call
- GPU calls outside a GPU context raise `SystemError`
- Background runs draw (`GPUOffScreen`, `batch_for_shader`, `draw_view3d`) after `gpu.init()`, and `gpu.texture.from_image` before it crashes Blender
- Background runs read `Region.data` `None`, and `RegionView3D.update()` crashes them
- `bpy.msgbus` keys name a declaring class and member (`(bpy.types.Object, "location")`) or one instance's member through `data.path_resolve("<member>", False)`
- Stock RNA writes notify msgbus subscribers coalesced after operators and outside undo
- `PERSISTENT` subscriptions survive ID remapping
- Python writes to registered `bpy.props` publish no msgbus notification whether or not they declare `update`, and `bpy.msgbus.publish_rna(key=<key>)` publishes them
- `bpy.ops` calls release the GIL and run any live Python thread inside the operator, and threads (`multiprocessing.Queue` feeders included) join before the next `bpy` call

## [07]-[BOUNDARIES]

Each entry point translates the result once into the host's form:

| [INDEX] | [ENTRY]                         | [HOST_FORM]                                                                |
| :-----: | :------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | `execute` with changes made     | Reports at their levels, a `WARNING` per skipped item, with `{'FINISHED'}` |
|  [02]   | `execute` with nothing changed  | Reason as `INFO` with `{'CANCELLED'}`                                      |
|  [03]   | `execute` with faults           | `ERROR` report per fault with `{'CANCELLED'}`, file as the body found it   |
|  [04]   | `poll`                          | `False` with `poll_message_set` of unmet requirements                      |
|  [05]   | `draw`, draw handler            | Failed part drawn as an alert row, rest drawn                              |
|  [06]   | Update callback, handler, timer | Faults stored for the extension to report, a raise printing its traceback  |
|  [07]   | `register`                      | One `RuntimeError` naming every fault after every acquired release runs    |
|  [08]   | `RenderEngine.render`           | `report({'ERROR'}, ...)` cancelling render                                 |
|  [09]   | `register_cli_command` function | Exit status 0 or 1                                                         |
|  [10]   | `--python` script               | `sys.exit` of `None`, or of faults joined by newlines for exit 1           |

Faults and exceptions stay apart:
- Host refusals raise `RuntimeError`, `TypeError`, or `ValueError` by call
- Host calls answering `None` (`links.new` across trees) or an invalid result (`NodeLink.is_valid` false) become faults at the call
- `CANCELLED` keeps the writes a body made before its fault outside undo, and dependent steps roll back through the body's `ExitStack` scope
- Python raises inside `RenderEngine.render` complete the render as `FINISHED`, and `error_set` cancels nothing
- `modifiers.move` past a pinned modifier keeps the order with no raise or report, and code reads the order after the call
- Helpers below an operator body return report entries in their result, and the body calls `report` once per entry

Values cross the boundary in host units and axes:
- Lengths are Blender units scaled by `unit_settings.scale_length` and angles radians, and decoded values hold SI quantities
- Display strings come from `bpy.utils.units.to_string(system, category, value, split_unit=True)` and parse through `bpy.utils.units.to_value`
- `to_value` reads `-` and `+` as arithmetic and unitless numbers as the base unit
- `bpy.utils.time_to_frame` and `time_from_frame` convert seconds through the context scene's frame rate
- Importers and exporters convert axes through `bpy_extras.io_utils.axis_conversion(from_forward=, from_up=, to_forward=, to_up=).to_4x4()`
- Stock exporters take `forward_axis` and `up_axis` (OBJ, STL, PLY), `axis_forward` and `axis_up` (FBX), or `export_yup` (glTF)
