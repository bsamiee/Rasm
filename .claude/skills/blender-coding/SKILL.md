---
name: blender-coding
description: "Use when writing or reviewing Python that runs inside Blender, covering bpy code from data references to host boundaries"
---

# [BLENDER_CODING]

Code runs on Blender's bundled Python 3.13 and takes no 3.14 or 3.15 form: imports stay eager, frozen mappings are `types.MappingProxyType`, `TypeForm` imports from `typing_extensions`, annotations read through `inspect.get_annotations(owner, eval_str=True)`, and text another program parses escapes each value at its format site (`shlex.join`) in place of a t-string. A package entry that also runs on 3.15 names its private modules in `__lazy_modules__`, which 3.13 ignores.

- Kinds, keys, and states are Blender's own values (`rna_enums` literals, `<Class>.bl_idname`)
- Rewrites keep every used behavior, failure cause, public export, and named value

[REFERENCES]:
- [01]-[DATA](references/data.md): Persistence, the store a value's lifetime selects and what keeps IDs and paths valid across saves and files
- [02]-[OPERATORS](references/operators.md): Operator classes, every way of running one reaching `execute` through its properties
- [03]-[GEOMETRY](references/geometry.md): Per-element data (vertices, keys, bones), reached through the store its mode and evaluation select
- [04]-[MATHUTILS](references/mathutils.md): Math over scene values, `mathutils` semantics against RNA and numpy, host members over hand-written geometry
- [05]-[LIFECYCLE](references/lifecycle.md): Code Blender runs on its own schedule, the state each body can read and where its writes and long work go
- [06]-[TYPING](references/typing.md): Types checkers and Blender's runtime both accept, one form for each place stubs and runtime disagree

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
|  [15]   | Dynamic enum `EnumPropertyItem`      | End of `enum_items` iteration listing it      | Item `identifier`                            |

Stale references fail by kind:
- Stale ID, `object_instances` item, `to_mesh()` mesh, and GeometrySet component references raise `ReferenceError` on every read
- Stale sub-data, `Keyframe`, `PropertyGroup`, and `mathutils` references read freed or reused memory, returning wrong values or crashing Blender
- Held generated input structs crash Blender at their next read after an interface write, a `default_value` write included

Writes to some members reset other members:
- Unit `system` writes reset `length_unit` and `mass_unit`, an equal value included, and leave `temperature_unit` and `time_unit`
- `rotation_mode` names the rotation member evaluation reads, and writes to the other rotation members store and change nothing
- `parent` writes keep `matrix_basis` and move the object, and a `matrix_world` copied before the write and assigned after it keeps the placement

Creation, removal, selection, and scope follow host rules:
- `.new()` returns the ID or node to keep, a name taken in its collection or tree suffixed `.001`, an ID name cut at 255 bytes
- IDs from `bpy.data.<collection>.new` and `new_from_object` meshes start with zero users until code links them, and saves drop zero-user IDs
- Material slots match data `materials`, `append` adding a slot on each sharing object and `pop(index=)` shifting higher face `material_index` down
- `slot.link = 'OBJECT'` overrides one object's slot starting at `None`, the data's entry and every other object's slot kept
- Material entries on the data count one user however many objects share the data
- `bpy.data.batch_remove(ids=...)` removes the IDs named alone, scanning users once where each `remove()` scans every user
- `batch_remove` crashes Blender on a removed ID, and held IDs resolve by `session_uid` before the call
- Objects come from context (`scene.objects`, `view_layer.objects`, a collection), `bpy.data.objects` holding orphans, other scenes, and linked data
- Current data comes from its owner (`scene.world`, `context.scene`), `bpy.data.<collection>[0]` naming the alphabetically first block
- `select_set(True)` raises `RuntimeError` for an object outside the view layer and changes nothing on a `hide_select` object
- `hide_set` is per view layer, `hide_viewport` global, `LayerCollection` `exclude` and `hide_viewport` per layer, `visible_get()` combining them

Removing an object leaves its data at zero users, and removals name objects with the data no other user holds in one `batch_remove`:

```python
def removed(objects: tuple[bpy.types.Object, ...], /) -> None:
    held = frozenset(objects)
    users = bpy.data.user_map(subset=tuple(item.data for item in objects if item.data is not None))
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
- `type(<struct>).__name__` names a refined subtype (`SunLight`), and dispatch matches base classes through `isinstance` or a class pattern
- Collection struct type tests (`ObjectModifiers`) never match a `bpy_prop_collection` value, and code binds the collection's own member
- RNA members of a class test through `name in T.bl_rna.properties` or `T.bl_rna.functions`
- `bpy.ops.<module>.<name>.get_rna_type()` raises `KeyError` for an operator the build lacks, its struct listing parameters in `properties`
- `enum_items` invokes context-free providers and exposes declared items for context-dependent enums
- `enum_items_static` lists declared items without calling a dynamic provider
- Bad dynamic enum values raise `TypeError` listing live items
- `UILayout.enum_item_name(data, prop, identifier)` reads a dynamic enum's live label and `""` for an identifier outside the live items
- Collections can hold `None` items (`texture_slots`), and iteration enumerates before skipping `None` to keep host indices

## [02]-[OPERATORS]

Per-object operator loops compound edits and costs, each call acting on the whole selection. Calls pay a context resolve, a view-layer update before the call and after `FINISHED`, and undo bookkeeping. Work takes a data API member where one does it, else a built-in operator, `bpy_extras`, `_bpy_internal`, or bundled add-on member Blender includes:

| [INDEX] | [WORK]                      | [FORM]                                                       |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | New ID                      | `bpy.data.<collection>.new`, then an explicit link           |
|  [02]   | Topology edit               | `bmesh` scope                                                |
|  [03]   | Per-element values          | `foreach_set` over attributes                                |
|  [04]   | Keys and drivers            | `keyframe_points.add` with `foreach_set`, `driver_add`       |
|  [05]   | Active and selected objects | `view_layer.objects.active`, `select_set`, set independently |
|  [06]   | Operator override           | Every member the operator reads, derived members included    |
|  [07]   | Procedure Blender includes  | Included member, operators under a `temp_override`           |
|  [08]   | Constraint applied          | Updated `matrix_world` copied, constraint removed, copy set  |
|  [09]   | Modifier stack applied      | `new_from_object` mesh as `data`, then `modifiers.clear()`   |
|  [10]   | Rigid body                  | `rigidbody.world_add`, then a link into its `collection`     |

Overrides reach the members an operator reads alone:
- `selected_objects` overrides leave `selected_editable_objects` reading the window's own selection
- Past any override, `object.mode_set` acts on `view_layer.objects.active` and exporter selected-only options on selection, returning `FINISHED`
- Overrides nest a region inside its area inside its window, and take no full-screen area or temporary screen
- `window=` alone sets workspace and screen and clears area and region
- `screen=` of another workspace switches the window's workspace and the active object's mode for the block
- Areas outside the screen and regions outside the area raise `TypeError` at `__enter__`, and members passed as `None` enter with no raise
- `temp_override` accepts unknown keywords with no raise
- Background runs hold `window` and `screen` but no area, editor operators overriding with a `screen.areas` `VIEW_3D` area and its `WINDOW` region

Operator calls fail by raising (`RuntimeError`, `TypeError`) or by a return set holding no success, and `called` turns both into a fault keeping the exception or set as `cause`. `object.join` reads `active_object` and `selected_editable_objects` and leaves joined meshes at zero users, one `batch_remove` after the call clearing them on a join and nothing on a failure:

```python
def called(operator: Callable[..., Returns], subject: str, /, **keywords: object) -> Outcome[Returns]:
    try:
        answer = operator("EXEC_DEFAULT", **keywords)
    except (RuntimeError, TypeError) as error:
        return Error(Fault.of(Reason.FAILED, subject, error))
    return Ok(answer).filter_with(lambda held: not held.isdisjoint({"FINISHED", "RUNNING_MODAL", "INTERFACE"}), partial(Fault.of, Reason.FAILED, subject))


def merged(context: bpy.types.Context, target: bpy.types.Object, items: Sequence[bpy.types.Object], /) -> Outcome[bpy.types.Object]:
    joined = tuple(item.data for item in items if item.data is not None)
    with context.temp_override(active_object=target, selected_editable_objects=[target, *items]):
        answer = called(bpy.ops.object.join, target.name)
    bpy.data.batch_remove(ids=tuple(block for block in joined if block.users == 0))
    return answer.map(lambda _: target)
```

Calls and their return sets:
- Script calls fill unset properties from scene settings (`scene.render.bake` for `object.bake`) and preferences (align, Edit Mode entry)
- `'INVOKE_DEFAULT'` runs `invoke` in a GUI session, and `wm.call_menu` crashes a background run under either call form
- Failing or raising `poll` makes the call raise `RuntimeError("Operator bpy.ops.<op>.poll() failed, context is incorrect")`
- `ERROR`, `ERROR_INVALID_INPUT`, `ERROR_INVALID_CONTEXT`, and `ERROR_OUT_OF_MEMORY` reports raise as one `RuntimeError` in either return set
- `FINISHED` means a built-in operator ran (`mesh.fill_grid` on an odd loop adds no face), and dependent work reads the data it wrote
- Calls return `{'FINISHED'}`, `{'RUNNING_MODAL'}`, `{'INTERFACE'}`, `{'CANCELLED'}`, or `{'PASS_THROUGH'}`
- `CANCELLED` and `PASS_THROUGH` mean failure, built-in operators returning `CANCELLED` with nothing to act on
- Work behind `RUNNING_MODAL` or `INTERFACE` finishes after the call in a GUI session, dependent reads running from a later timer step
- `window.workspace` and file-browser `params` writes apply one event-loop pass later, dependent work running from a timer or modal step
- `area.type` and `show_region_*` writes apply inside the write

Created IDs are the `bpy.data.all_ids` difference across a call in place of a requested name or `selected_objects`. Raising calls keep their writes, and one function serving every importer removes the created set on a failure:

```python
def imported(importer: Callable[..., Returns], source: Path, /) -> Outcome[frozenset[bpy.types.ID]]:
    before = frozenset(bpy.data.all_ids)
    answer = called(importer, str(source), filepath=str(source))
    created = frozenset(bpy.data.all_ids) - before
    bpy.data.batch_remove(ids=tuple(created if answer.is_error() else ()))
    return answer.map(lambda _: created)
```

`imported(bpy.ops.wm.obj_import, source)` returns the created mesh and object, and an unreadable file returns one fault holding the importer's report.

## [03]-[PROPERTIES]

Settings, operator properties, and preferences are `bpy.props` annotations on `PropertyGroup`, `Operator`, and `AddonPreferences` classes. Properties take Blender's keywords (`min`, `max`, `soft_min`, `soft_max`, `step`, `precision`, `subtype`), a `use_<name>` `BoolProperty` beside each value it switches. Copies of an RNA member take every keyword its RNA states, enum items as 5-tuples keeping each item's RNA `value` and floats their `unit` in place of `subtype`. Update callbacks act on the owning ID through `rasm_bpy.data.id_data` in place of the active item:

```python
def copied(owner: type[Struct], member: str, /, *, update: Callable[[bpy.types.PropertyGroup, bpy.types.Context], None] | None = None) -> bpy.props._PropertyDeferred:
    described = owner.bl_rna.properties[member]
    match described:
        case bpy.types.EnumProperty(enum_items=items, default=default):
            definition = partial(bpy.props.EnumProperty, items=tuple((item.identifier, item.name, item.description, item.icon, item.value) for item in items), default=default)
        case bpy.types.BoolProperty(array_length=0, default=default):
            definition = partial(bpy.props.BoolProperty, default=default)
        case bpy.types.FloatProperty(array_length=0) as real:
            definition = partial(
                bpy.props.FloatProperty,
                unit=real.unit,
                min=real.hard_min,
                max=real.hard_max,
                soft_min=real.soft_min,
                soft_max=real.soft_max,
                step=real.step,
                precision=real.precision,
                default=real.default,
            )
        case _:
            raise TypeError(f"{owner.__name__}.{member} is a {described.type} property")
    return definition(name=described.name, description=described.description, update=update)


def retagged(self: bpy.types.PropertyGroup, _context: bpy.types.Context, /) -> None:
    for owner in id_data(bpy.types.ID, self).to_list():
        owner.update_tag()


light = partial(copied, bpy.types.Light, update=retagged)


class Lamp(bpy.types.PropertyGroup):
    kind: Annotated[str, light("type")]
    use_reach: Annotated[bool, light("use_custom_distance")]
    reach: Annotated[float, light("cutoff_distance")]
```

`kind` starts at `POINT` and stores `AREA` as its `Light.type` value 4, where 3-tuples store position 3. `copied(<class>, <member>)` copies static or context-free RNA enum items, booleans, and scalar floats; `light` binds owner and callback once:
- `partial` binds each kind's keywords and one call adds name, description, and callback, ty refusing a `**` dict of mixed values at each typed `bpy.props` parameter
- Scalar RNA floats state array subtypes (`XYZ`, `COLOR`), which `register_class` refuses on `FloatProperty` with `ValueError` and drops the property

`bpy.props` definitions take the form Blender registers:
- `poll` and `update` take function objects with exact positional arity, defaulted parameters counted, and a factory returns the closure
- Bad arguments print `TypeError` at `register_class` and drop the property from the registered class, `int32` overflows printing `OverflowError`
- Float bounds past `float32` store as infinity, and `min > max` and out-of-range defaults register with no error
- Operator properties hold IDs by name or `session_uid`, ID `PointerProperty` printing `ValueError` at `register_class` and dropping from operator
- `PointerProperty` takes ID and `PropertyGroup` types, `CurveMapping`, `ColorRamp`, `Scopes`, and `Histogram` coming from an owning ID
- `PointerProperty` `poll` filters UI assignment alone, and assigning an embedded ID (`Scene.collection`, `Material.node_tree`) raises `RuntimeError`
- Float property units follow `subtype` (`DISTANCE` and `XYZ_LENGTH` read `unit` `LENGTH`), a mismatched `unit` logging at each `subtype` read
- `get` and `set` hold values stored outside Blender, `set` needing `get` and `get` alone making the property read-only
- `set_transform(self, new_value, stored_raw_value, is_set)` results clamp to hard bounds
- `get` and `get_transform(self, stored_value, is_set)` results stay unclamped
- `update`, `get`, `set`, and transform callbacks can run in a threaded context per data-block, call no operator, and touch their own ID alone
- Dynamic `items` callbacks accept a `None` context and number items stably per identifier, with an integer `default`
- Callback closures retain canonical identifier, label, and description strings for each registered property's lifetime
- Native RNA owns callback references; numeric icons, values, and past row tuples need no retention
- `FILE_PATH` and `DIR_PATH` properties accepting `//` declare `PATH_SUPPORTS_BLEND_RELATIVE`
- Operator property update callbacks receive properties alone as `self`, with the window manager as `id_data`

Registered values read and write through Blender:
- Registered properties on IDs use system storage outside `ID.keys()`; nested `PropertyGroup` members share the group's storage
- `ob["prop"] = value` on a registered name creates a separate custom property
- `property_unset` resets a value, and `is_property_set(name)` counts a value reused from the last run as unset, `ghost=False` counting it as set
- Captures name members, writable-member walks reach view state (`cursor_location`, `is_dirty`) and `PASSWORD` tokens (`auth_token`, `access_token`)
- `bl_rna.properties[name].is_readonly` is static, and `struct.is_property_readonly(name)` reports the member's read-only state in its current context

## [04]-[UNDO]

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
- Undo re-reads each ID its step touched at its old address, held references to them raising `ReferenceError` while untouched IDs keep working
- Undo restores registered groups, ID properties, and compositor inputs and runs no `update` callback
- Each undo push marks the file unsaved
- `window_manager.undo_stack` lists `steps`, `active_index`, and `active`, reading `None` before the first push

## [05]-[CALLBACKS]

Handler, timer, msgbus, and modal bodies run in a context Blender chose:
- Blender passes one argument to a handler function of one positional parameter and pads every other callable with `None`
- `load_*` and `save_*` handlers receive `""` as the startup file's path
- Depsgraph `*_post` handlers read the scene and depsgraph they receive
- `frame_set` fires `frame_change_pre` and `frame_change_post` alone
- `frame_change_pre` writes data for the frame, and `frame_change_post` handlers read evaluated results
- Raising native timers unregister permanently, and a raising handler prints its traceback while later handlers run
- Handlers remove only themselves from their list during their call
- GPU calls outside a GPU context raise `SystemError`
- Background runs draw (`GPUOffScreen`, `batch_for_shader`, `draw_view3d`) after `gpu.init()`, and `gpu.texture.from_image` before it crashes Blender
- Background runs read `Region.data` `None`, and `RegionView3D.update()` crashes them
- `bpy.msgbus` keys name a class and member (`(bpy.types.Object, "location")`) or one instance's member through `data.path_resolve("<member>", False)`
- Built-in RNA writes notify msgbus subscribers coalesced after operators and outside undo
- `PERSISTENT` subscriptions survive ID remapping
- Python writes to registered `bpy.props` publish no msgbus notification, `update` or not, and `bpy.msgbus.publish_rna(key=<key>)` publishes them
- `bpy.ops` calls release the GIL to live Python threads mid-operator, and threads (`multiprocessing.Queue` feeders included) join before a `bpy` call

## [06]-[BOUNDARIES]

Each entry point translates the result once into Blenders form:

| [INDEX] | [ENTRY]                         | [RESULT]                                                                   |
| :-----: | :------------------------------ | :------------------------------------------------------------------------- |
|  [01]   | `execute` with changes made     | Reports at their levels, a `WARNING` per skipped item, with `{'FINISHED'}` |
|  [02]   | `execute` with nothing changed  | Reason as `INFO` with `{'CANCELLED'}`                                      |
|  [03]   | `execute` with faults           | `ERROR` report per fault with `{'CANCELLED'}`, file as the body found it   |
|  [04]   | `poll`                          | `False` with `poll_message_set` of unmet requirements                      |
|  [05]   | Update callback, handler, timer | Faults logged through a structlog logger, a raise printing its traceback   |
|  [06]   | `RenderEngine.render`           | `report({'ERROR'}, ...)` cancelling render                                 |
|  [07]   | `--python` script               | `sys.exit` of `None`, or of faults joined by newlines for exit 1           |

Helpers below an operator body return faults as the error and skipped-item faults as the success value, and the body reports each once:

```python
def answered(operator: bpy.types.Operator, outcome: Outcome[tuple[Fault, ...]], /) -> Returns:
    match outcome:
        case Result(tag="ok", ok=skipped):
            for fault in skipped:
                operator.report({"WARNING"}, fault.message)
            return {"FINISHED"}
        case Result(error=faults):
            for fault in faults:
                operator.report({"ERROR"}, fault.message)
            return {"CANCELLED"}
```

`--python` scripts read their arguments after `--` in `sys.argv`, Blender's own flags filling the entries before it.

Blender calls that raise nothing decide the result through their return:
- `None` returns and invalid results (false `NodeLink.is_valid`, targetless `Constraint.is_valid`) are faults at the call
- `CANCELLED` keeps the writes a body made before its fault outside undo, and dependent steps roll back through the body's `ExitStack` scope
- Python raises inside `RenderEngine.render` complete the render as `FINISHED`, and `error_set` cancels nothing
- Modifier reorder plans keep `use_pin_to_last` modifiers after unpinned ones

Values cross the boundary in Blenders units and axes:
- Lengths are Blender units scaled by `unit_settings.scale_length` and angles radians, and decoded values hold SI quantities
- Display strings come from `bpy.utils.units.to_string(system, category, value, split_unit=True)` and parse through `bpy.utils.units.to_value`
- `to_value` reads `-` and `+` as arithmetic and unitless numbers as the base unit
- `bpy.utils.time_to_frame` and `time_from_frame` convert seconds through the context scene's frame rate
- Importers and exporters convert axes through `bpy_extras.io_utils.axis_conversion(from_forward=, from_up=, to_forward=, to_up=).to_4x4()`
- Built-in exporters take `forward_axis` and `up_axis` (OBJ, STL, PLY), `axis_forward` and `axis_up` (FBX), or `export_yup` (glTF)
