---
name: blender-coding
description: "Use when writing or reviewing Python that runs inside Blender, covering bpy code from data references to host boundaries"
---

# [BLENDER_CODING]

Covers `bpy` code decisions under workspace standards, from holding a data reference to translating a fault at a host entry point.

[REFERENCES]:
- [01]-[DATA](references/data.md): Storage owner per value lifetime, created IDs, library loads, paths, custom property values
- [02]-[OPERATORS](references/operators.md): Operator classes on `Operator`, from conditions and invoke forms to modal steps and placements
- [03]-[GEOMETRY](references/geometry.md): Evaluated reads, BMesh, mesh attributes, buffer paths and costs, and animation channels
- [04]-[LIFECYCLE](references/lifecycle.md): Callback bodies across file loads, updates, deferred writes, frames, drivers, and worker processes
- [05]-[TYPING](references/typing.md): Annotation forms under eager evaluation, stub declarations runtime contradicts, and the code form each takes

## [01]-[OWNERS]

Code calls an operation's `blender.bpy` owner in place of the `bpy` member it wraps.

### [01.1]-[DATA_AND_COMMANDS]

Data, command, and callback needs:

| [INDEX] | [NEED]                                        | [OWNER]                                                                               |
| :-----: | :-------------------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | Fallible host result, independent results     | `faults.BlenderResult[T]`, `blender.imaging.faults` `accumulated` and `paired`        |
|  [02]   | Operator call                                 | `commands.calls.called`                                                               |
|  [03]   | Operator class over a typed record            | `commands.operators.Operator`, its body `Call[T] -> BlenderResult[Done \| Unchanged]` |
|  [04]   | Modal gesture, timer step, status             | `commands.modal.Modal`                                                                |
|  [05]   | RNA write checked against its live owner      | `data.rna.descriptors.written`, dotted identifiers included                           |
|  [06]   | Member writes in host order                   | `data.rna.descriptors.assigned`, `data.rna.snapshots.Snapshot` with a restore         |
|  [07]   | Struct path inside its ID, struct at a path   | `data.rna.descriptors` `id_path`, `struct_at`                                         |
|  [08]   | Context root of a type                        | `data.rna.descriptors.root_in`, `Call.root` inside an operator body                   |
|  [09]   | Requirement over context with a poll message  | `data.rna.conditions` `Condition` cases and `check`                                   |
|  [10]   | Keyframes and drivers of one channel          | `data.rna.animation.Animation`, `sampled`                                             |
|  [11]   | ID held across calls                          | `data.ids.store.Handle`                                                               |
|  [12]   | Rename that refuses a taken name              | `data.ids.store.named`                                                                |
|  [13]   | Created ID with a saved lifetime and reuse    | `data.ids.ownership.Ownership`                                                        |
|  [14]   | Evaluated depsgraph, mesh, instances, or hit  | `data.ids.evaluation` `evaluated`, `meshed`, `Instance.read`, `hit`                   |
|  [15]   | Settings record bound to RNA properties       | `settings.properties.RecordGroup`, `settings.attachments.Attachment`                  |
|  [16]   | Library load, another file's data             | `files.blends` `sourced` and `inspected`, `files.libraries.blocks_loaded`             |
|  [17]   | IDs written to another file with dependencies | `files.libraries.library_written`                                                     |
|  [18]   | External paths with presence, portable copy   | `files.blends` `dependencies`, `packed`                                               |
|  [19]   | Handler, timer, or msgbus callback            | `extension.events` handler rows, `appended`, `Timer`, `Subscription`                  |
|  [20]   | Callback faults, deferred main-thread write   | `extension.reports.Reports` `reported`, `deferred`, `hold`                            |
|  [21]   | Extension registration                        | `extension.registration.scope`                                                        |

### [01.2]-[NODES]

Node tree needs:

| [INDEX] | [NEED]                                        | [OWNER]                                                                                 |
| :-----: | :-------------------------------------------- | :-------------------------------------------------------------------------------------- |
|  [01]   | Node of a class, socket by identifier, link   | `nodes.trees` `created`, `socket_at`, `joined`                                          |
|  [02]   | Group node pointed at a group                 | `nodes.trees.grouped`, a group holding the node's own tree refused                      |
|  [03]   | Multi-input link order, active Group Output   | `nodes.trees` `ordered`, `active_output`                                                |
|  [04]   | Cache key over a tree or a node's upstream    | `nodes.trees.fingerprinted`                                                             |
|  [05]   | Node tree of an ID, its `bpy.data` holder     | `nodes.trees.tree_of`, `data.ids.store.holder`                                          |
|  [06]   | Desired graph                                 | `nodes.graphs` `Graph`, `Node`, `Fragment`, `NodeGroup.built`, `node`, `menu`, `nested` |
|  [07]   | Reconciliation planned, then written          | `nodes.changes` `changes`, `reconciled`, `owned`                                        |
|  [08]   | Owned group reused or built                   | `nodes.changes.made`                                                                    |
|  [09]   | Node content and socket defaults              | `nodes.changes` `stored`, `valued`                                                      |
|  [10]   | Group interface from a record, its versioning | `nodes.interface` `Interface.of`, `MenuItems.of`, `interfaced`, `revised`               |
|  [11]   | Group spliced after an editor's active node   | `nodes.editing` `Chosen.of`, `inserted`                                                 |
|  [12]   | Owned node layout once nodes draw             | `nodes.placement.Arranger`                                                              |
|  [13]   | Compositor effect input by key                | `render.effects.records.entry_input`                                                    |

### [01.3]-[INTERFACE]

Interface needs, each owner a row `extension.registration.scope` acquires:

| [INDEX] | [NEED]                                      | [OWNER]                                                                                    |
| :-----: | :------------------------------------------ | :----------------------------------------------------------------------------------------- |
|  [01]   | Panel in a tab, sidebar, region, or popover | `ui.surfaces.panels.Panel` at `Tab`, `Sidebar`, `Joined`, `Child`, `EditorRegion`          |
|  [02]   | Panel as top bar popover or docked          | `ui.surfaces.panels.Panel` at `PopoverPanel`, `Docked`                                     |
|  [03]   | Collapsible section inside a draw           | `ui.layout.sections` `Single`, `Each`, `ui.layout.values` `Store`, `Hosted`                |
|  [04]   | Menu, pie, keyed menu, preset menu          | `ui.surfaces.menus.Menu` with a `Dropdown`, `Keyed`, or `Pie` body, `PresetMenu`           |
|  [05]   | Registered menu, panel, or shelf popup      | `commands.calls.opened` over `MenuPopup`, `PanelPopup`, `ShelfPopup`                       |
|  [06]   | Popup or popover a callback draws           | `commands.calls.opened` over `DrawnMenu`, `DrawnPopover`                                   |
|  [07]   | UI list with filter and sort                | `ui.layout.lists.ItemList`                                                                 |
|  [08]   | Rows, fields, labels, progress              | `ui.layout.rows` `Property`, `Caption`, `Label`, `Text`, `Progress`, `Selector`, `Tree`    |
|  [09]   | Operator, popover, and menu buttons         | `ui.layout.rows` `OperatorButton`, `Popover`, `Pulldown`, `invoking`                       |
|  [10]   | Templates                                   | `ui.layout.templates` `Wheel`, `Pages`, `IconView`, `Preview`, `ImageFormat`, `Shortcut`   |
|  [11]   | Report banner, cursor progress              | `ui.layout.status` `Status`, `progressed`                                                  |
|  [12]   | Generated previews, package icons           | `ui.resources.previews.Previews`, `ui.resources.icons.Icons`                               |
|  [13]   | Theme colors, interface metrics             | `ui.resources.theme` `Theme`, `Metrics.read`                                               |
|  [14]   | GPU texture from pixels                     | `ui.resources.textures` `uploaded`, `TEXTURES`                                             |
|  [15]   | Draw handler over cached state              | `ui.drawing.handlers` `Overlay`, `Drawing`, `Part`, `Watch`, `Signal`                      |
|  [16]   | 2D drawing in a region                      | `ui.drawing.canvas.Canvas.of`                                                              |
|  [17]   | Offscreen draw, view capture, buffer read   | `ui.drawing.offscreen` `Offscreen.drawn`, `Offscreen.viewed`, `captured`                   |
|  [18]   | Gizmo group                                 | `ui.surfaces.gizmos` `Gizmos`, `Linked`                                                    |
|  [19]   | Workspace tool                              | `ui.surfaces.tools.Tools`, `Tools.invocation`                                              |
|  [20]   | Areas, regions, workspaces                  | `ui.screens` `areas.Located`, `areas.AreaQuery`, `areas.Arrangement`, `regions.RegionView` |
|  [21]   | Workspace switch                            | `ui.screens.workspaces.Activation`                                                         |
|  [22]   | Preference page                             | `ui.surfaces.preferences.PreferencesPage`                                                  |
|  [23]   | Rows on a stock menu, panel, or header      | `ui.surfaces.appends.Append` with a type or `ContextMenu` target                           |
|  [24]   | Asset shelf                                 | `files.shelves.Shelf`, `ui.layout.rows.ShelfPopover`                                       |
|  [25]   | Redraw of every area                        | `extension.reports.redrawn`                                                                |
|  [26]   | Translation table                           | `extension.registries.Translations`                                                        |

## [02]-[DATA]

Host references go stale after their event, and code holds the form that survives:

| [INDEX] | [REFERENCE]                          | [STALE_AFTER]                                   | [HOLD]                                  |
| :-----: | :----------------------------------- | :---------------------------------------------- | :-------------------------------------- |
|  [01]   | ID                                   | `remove()`, an undo step touching it, file load | `Handle`, `IdName` across file loads    |
|  [02]   | Vertex, bone, point, UV, attribute   | `mode_set`, container growth, ID removal        | Index or name, re-read after the change |
|  [03]   | `CollectionProperty` item            | `add()` on its collection                       | Index, re-read after each `add()`       |
|  [04]   | Grease Pencil stroke or point        | `remove_strokes`, `resize_strokes`              | `frame.drawing` read again              |
|  [05]   | `Keyframe`                           | Next insert on its F-curve                      | `keyframe_points` re-read per insert    |
|  [06]   | `mathutils` value read from RNA      | Owner write (read live), owner removal          | `.copy()`                               |
|  [07]   | `PropertyGroup` on an ID             | Undo                                            | Re-resolved through its ID              |
|  [08]   | Evaluated ID                         | `Depsgraph.update()`, rewriting it in place     | Values copied out per evaluation        |
|  [09]   | `object_instances` item              | Next iteration step                             | `Instance.read` copies                  |
|  [10]   | `to_mesh()` result                   | Re-evaluation, `to_mesh()`, `to_mesh_clear()`   | `meshed` scope                          |
|  [11]   | GeometrySet component                | Release of its GeometrySet                      | GeometrySet bound for the read          |
|  [12]   | Struct with a class a write refines  | `Light.type`, `dimensions`, `socket_type`       | Re-resolved through `id_path`           |
|  [13]   | Generated `properties.inputs` struct | Group assignment, any interface write           | `struct_at(owner, "properties.inputs")` |
|  [14]   | Pointer an operator re-targets       | `object.convert`, importer `obj.data` swaps     | Read through its owner after the call   |
|  [15]   | `ui_styles` item                     | Theme reset, preset load                        | Re-read from its owner                  |
|  [16]   | Dynamic enum `EnumPropertyItem`      | End of `enum_items` iteration listing it        | Item `identifier`                       |

Stale references fail by kind:
- Stale ID, `object_instances` item, `to_mesh()` mesh, and GeometrySet component references raise `ReferenceError` on every read
- Stale sub-data, `Keyframe`, `PropertyGroup`, and `mathutils` references read freed or reused memory with no raise, and reads return wrong values or crash Blender
- Held generated input structs crash Blender at their next read after an interface write, a `default_value` write included

Writes to a governing member reset governed members, and member maps write through `assigned` in host order:
- Unit `system` writes reset `length_unit` and `mass_unit`, an equal value included, and leave `temperature_unit` and `time_unit`
- `rotation_mode` names the rotation member evaluation reads, and writes to the other rotation members store and change nothing
- Driver targets write `id_type` before `id`, an `id` of another type raising `TypeError`

Creation, removal, and work sets follow host lifetimes:
- `.new()` returns the ID or node to keep, a name taken in its collection or tree suffixed `.001`, an ID name cut at 255 bytes
- IDs from `bpy.data.<collection>.new` and `new_from_object` meshes start with zero users until code links them, and saves drop zero-user IDs
- Removing an object leaves its data at zero users, and removals name objects with their unshared data
- `bpy.data.batch_remove(ids=...)` removes only the IDs named
- `remove()` scans every user per call, and removals of more than one ID run through one `batch_remove`
- `batch_remove` crashes Blender on an already removed ID, and held IDs resolve through `Handle` before the call
- Work sets come from the context scope (`context.scene.objects`, `view_layer.objects`, a collection), `bpy.data.<collection>` holding other scenes, orphans, and linked data
- Current data comes from its owner (`scene.world`, `context.scene`), `bpy.data.<collection>[0]` naming the alphabetically first block
- `select_set(True)` raises `RuntimeError` for an object outside the view layer and changes nothing on a `hide_select` object
- Visibility has one owner per scope: `hide_set` per view layer, `hide_viewport` on objects and collections globally, `LayerCollection.exclude` and `hide_viewport` per layer, `visible_get()` combining them

Identity and lookup follow host keys:
- `bpy.data.<collection>[(name, None)]` reads local data alone, `(name, <library path>)` linked data
- Name, index, `in`, and `get` lookups on `bpy.data` collections and `Handle.resolve` scan linearly, and operations resolving many build one `{session_uid: id}` map
- `bpy.data.all_ids` iterates every ID
- `bpy.data.<collection>` iterates in name order, and loops that rename iterate `bpy.data.<collection>[:]`
- Loops writing object visibility iterate `Collection.all_objects[:]`
- Default names (`"Material Output"`) translate with the interface language, and code holds the returned reference or matches `type`
- Embedded node trees sit outside `bpy.data.node_groups` with `id_data` naming each tree, and handles and marks key on the tree's holder from `holder`
- Names inside data paths and msgbus paths escape through `bpy.utils.escape_identifier`, and generated expression text quotes through `repr`
- Structs compare with `==`
- Caches key by `Handle`, its `session_uid` holding across renames, undo, and an undone removal while `as_pointer()` addresses repeat
- Struct wrappers rebuild per access and ID wrappers per class refinement
- `bl_rna.identifier` names a refined subtype (`SunLight`), and dispatch matches base classes through `isinstance` or a class pattern
- Collection struct type tests (`ObjectModifiers`) never match a `bpy_prop_collection` value, and code binds the collection's own member
- `data.rna.descriptors.subclass` builds a native class on first read, and `bl_rna_get_subclass_py` answers `None` for one nothing read yet
- RNA members of a class test through `name in T.bl_rna.properties` or `T.bl_rna.functions`
- Pinned RNA members resolve once through module-level `Descriptor.declared`, and members named at run time through `Descriptor.resolve`
- Operators test through `"<name>" in dir(bpy.ops.<module>)` and list parameters through `get_rna_type().properties`, `hasattr` answering true for any name
- Dynamic enums (engine, color management, OCIO views) list a static placeholder in `enum_items` (`BLENDER_EEVEE`, `NONE`), and `written` validates them live
- `UILayout.enum_item_name(data, prop, identifier)` reads a dynamic enum's live label and `""` for an identifier outside the live items
- Collections can hold `None` items (`texture_slots`), and iteration enumerates before skipping `None` to keep host indices

## [03]-[OPERATORS]

Operators read context and act on the whole selection, and a per-object loop compounds edits and the costs each call pays:
- Context resolve
- View-layer update before the call and again after a `FINISHED` answer
- Undo bookkeeping

Work takes its data API form, and an operator only where none exists:

| [INDEX] | [WORK]                      | [FORM]                                                                                 |
| :-----: | :-------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | New ID                      | `bpy.data.<collection>.new`, then an explicit link                                     |
|  [02]   | Topology edit               | `bmesh` scope                                                                          |
|  [03]   | Per-element values          | `foreach_set` over attributes                                                          |
|  [04]   | Keys and drivers            | `Animation`                                                                            |
|  [05]   | Active and selected objects | `view_layer.objects.active`, `select_set`, set independently                           |
|  [06]   | Work no data API reaches    | `called` with an override of every member the operator reads, derived members included |

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
def merged(context: bpy.types.Context, target: bpy.types.Object, items: tuple[bpy.types.Object, ...]) -> BlenderResult[None]:
    joined: tuple[bpy.types.ID, ...] = tuple(item.data for item in items if item.data is not None)
    return called(bpy.ops.object.join, {}, override=Some(context.temp_override(active_object=target, selected_editable_objects=[target, *items]))).map(
        lambda _: bpy.data.batch_remove(ids=tuple(block for block in joined if block.users == 0))
    )
```

Calls and their answers:
- Script calls fill unset properties from scene settings (`object.bake` from `scene.render.bake`) and user preferences (align, Edit Mode entry)
- Calls pass every value their result depends on
- `'INVOKE_DEFAULT'` runs `invoke`, and `wm.call_menu` crashes a background run under either call form
- Failing `poll` raises `RuntimeError("Operator bpy.ops.<op>.poll() ...")`, and `called` maps it to `BlenderOperatorFault`
- `ERROR`, `ERROR_INVALID_INPUT`, `ERROR_INVALID_CONTEXT`, and `ERROR_OUT_OF_MEMORY` reports raise `RuntimeError` under either return set
- `INFO` and `WARNING` reports print
- `FINISHED` reports that a stock operator ran (`mesh.fill_grid` on an odd loop adds no face), and work depending on its effect reads the data it wrote
- `called` answers `FINISHED`, `RUNNING_MODAL`, and `INTERFACE` as `Ok` and `CANCELLED` or `PASS_THROUGH` as `BlenderOperatorFault`
- Work behind `RUNNING_MODAL` or `INTERFACE` finishes after the call in a GUI session and never in a background run, dependent reads running from a later timer step
- Stock operators can answer `CANCELLED` on an empty work set, and code tests the work set before the call
- Created IDs come from the `bpy.data.<collection>` difference before and after a call, each read by its own `name` in place of the requested one or `selected_objects`
- Operators that raise keep the writes made before the raise, and callers remove the created set through `batch_remove`
- `window.workspace` and file-browser `params` writes apply one event-loop pass later, dependent work running from a timer or modal step
- `area.type` and `show_region_*` writes apply inside the write

## [04]-[EVALUATION]

Original data holds no modifier, constraint, or animation result, and each read or write takes its path:

| [INDEX] | [NEED]                                         | [PATH]                                                                           |
| :-----: | :--------------------------------------------- | :------------------------------------------------------------------------------- |
|  [01]   | Modified or animated state                     | `evaluated_get` on one `evaluated(view_layer)` per operation                     |
|  [02]   | Selection, identity, any write                 | `.original` of the evaluated ID                                                  |
|  [03]   | `matrix_world`, `dimensions`, `bound_box`      | `evaluated(view_layer)` after the data write                                     |
|  [04]   | World extents                                  | `matrix_world` over the evaluated geometry                                       |
|  [05]   | Keyed F-curve value at other frames            | `sampled(owner, path, frames)`                                                   |
|  [06]   | Evaluated state at another frame               | `frame_set` on the window scene, its frame restored after                        |
|  [07]   | Instances                                      | `Instance.read(depsgraph)`                                                       |
|  [08]   | Evaluated mesh                                 | `meshed(obj, depsgraph)` scope                                                   |
|  [09]   | Evaluated attributes, instance references      | `obj.evaluated_get(depsgraph).evaluated_geometry()`, bound for the read          |
|  [10]   | Persistent evaluated mesh                      | `new_from_object(evaluated, preserve_all_data_layers=True, depsgraph=depsgraph)` |
|  [11]   | Result after a write that tags nothing         | `id.update_tag()` on the written ID, then evaluation                             |
|  [12]   | Result after a modifier input write            | `obj.update_tag()`, then evaluation                                              |
|  [13]   | Original mesh read in Edit Mode                | `obj.update_from_editmode()` first                                               |
|  [14]   | `view_layer.objects` after a link or `exclude` | `evaluated(view_layer)` first                                                    |

Evaluation follows host rules:
- Writes to an evaluated ID succeed with no raise and drop at the next evaluation
- RNA writes to built-in members tag their owner, while `id["key"]` writes, script writes to registered `bpy.props`, `CurveProfile` point writes with `update()`, `PointCloud.resize`, and `GreasePencilFrames.move` tag nothing
- `scene.frame_current = n` applies at the next `evaluated(view_layer)`, and `frame_set` evaluates at the call
- `evaluated(view_layer)` serves every scene's layers, and `context.evaluated_depsgraph_get()` returns the active layer's depsgraph
- `evaluated_depsgraph_get()` evaluates at viewport settings, and exports needing render results pass `evaluation_mode` (USD, Alembic, PLY, STL) or `export_eval_mode` (OBJ)
- `evaluated_geometry()` raises `TypeError` on an original or an object without geometry, and `instances_pointcloud()` reads `None` with no instances
- Instances stay out of the evaluated mesh and `dimensions`
- `Object.hide_viewport`, `Collection.hide_viewport`, and `LayerCollection.exclude` take objects out of the depsgraph, and `hide_set` objects still evaluate
- Evaluated meshes hold their source mesh's name, and code branches on the object it evaluated
- Reads copy through `foreach_get` into numpy buffers inside the `meshed` scope, the mesh clearing at scope exit

## [05]-[PROPERTIES]

`RecordGroup` binds settings records to RNA properties for `Operator.form`, `Attachment`, and preference pages:
- `str | None` fields bind as text holding `""` for absence, and `ID | None` fields as pointers
- Other optional settings are `Gated[T]` fields from `blender.imaging.parameters`, an enabled flag beside its value
- Alternatives are `Case` subclasses with integer tags
- Menu choices are `Choice` members with the persistent integers files store

`bpy.props` definitions take the form the host registers:
- `poll` and `update` take function objects with exact positional arity, defaulted parameters counted, and a factory returns the closure
- Bad arguments print and drop the property at `register_class`, integer bounds past `int32` printing `OverflowError`
- Float bounds past `float32` store as infinity, and `min > max` and out-of-range defaults register with no error
- Assignment clamps to hard bounds with no raise and leaves soft bounds unenforced, and `written` refuses a value outside the hard range
- Socket defaults take `float32` limits as hard bounds, and a record's `msgspec.Meta` holds the domain bound
- Operator records hold an ID as `IdName`, an ID `PointerProperty` on an operator printing a `ValueError` at `register_class` and the operator registering without it
- `PointerProperty` takes ID and `PropertyGroup` types, `CurveMapping`, `ColorRamp`, `Scopes`, and `Histogram` coming from an owning ID
- `PointerProperty` `poll` filters UI assignment alone, and assigning an embedded ID (`Scene.collection`, `Material.node_tree`) raises `RuntimeError`
- `FloatProperty` definitions declaring `unit` declare a matching `subtype`, a missing subtype logging an enum mismatch on each read of the definition's `subtype`
- `get` and `set` hold values stored outside Blender
- `set` needs `get`, and `get` alone makes the property read-only
- `get_transform(self, stored_value, is_set)` and `set_transform(self, new_value, stored_raw_value, is_set)` transform stored values
- `set_transform` results clamp to the hard range, `get` and `get_transform` results reach readers unclamped, and a wrong type prints `TypeError`
- `update`, `get`, `set`, and transform callbacks can run in a threaded context per data-block, call no operator, and touch their own ID alone
- `Choices` builds dynamic enum items with live strings, a result for a `None` context, and numbers stable per identifier, a dynamic `EnumProperty` taking an integer default
- `ENUM_FLAG` items number by powers of 2
- `FILE_PATH` and `DIR_PATH` properties accepting `//` declare `PATH_SUPPORTS_BLEND_RELATIVE`
- Update callbacks act on `self` and `self.id_data` in place of the active item
- Operator record update callbacks receive properties alone as `self`, with the window manager as `id_data`

Registered values read and write through the host:
- Values read and write as attributes and sit in system properties outside `ID.keys()`, `bl_system_properties_get()` listing them
- `ob["prop"] = value` on a registered name creates a separate custom property
- `property_unset` resets a value, and `is_property_set(name)` counts a value reused from the last run as unset, `ghost=False` counting it as set
- `Snapshot.capture` takes every writable member, view state (`cursor_location`, `is_dirty`) and `PASSWORD`-subtype tokens (`auth_token`, `access_token`) included, and a `selection` narrows it
- Whole-mesh captures cost 1.1 to 1.9 ms per face, and caches over geometry key on `foreach_get` buffers beside a capture selecting the block's own value members
- `bl_rna.properties[name].is_readonly` is static, and `struct.is_property_readonly(name)` answers whether the member is editable now
- Data paths (`driver_add`, `keyframe_insert`, `path_resolve`) name a registered property by identifier, `["name"]` resolving ID properties alone
- `path_from_id()` raises `ValueError` for structs with no ID path (preferences, `Region`, `RegionView3D`, `CurveMapping`, `Stereo3dFormat`)
- `id_path` joins a nested struct's member name to a holding struct's path, and `path_from_module()` returns a full path with the ID

## [06]-[UNDO]

Undo restores `bpy.data` alone, and each change pushes its step by form:

| [INDEX] | [CHANGE]                                 | [STEP]                                          | [FORM]                                     |
| :-----: | :--------------------------------------- | :---------------------------------------------- | :----------------------------------------- |
|  [01]   | Operator body that edits data            | One per `FINISHED` from UI, keymap, `undo=True` | `Operator.undo` on                         |
|  [02]   | Script `bpy.ops` call, `called` default  | None, writes joining the next step              | `called(..., undo=True)`                   |
|  [03]   | Operator called inside an `UNDO` body    | None, the outer step holding every write        | Plain nested `called`                      |
|  [04]   | Window manager, screen, preference write | None, undo restoring no such state              | `Operator.undo` off                        |
|  [05]   | Steps the operator names itself          | One per `ed.undo_push(message=...)`             | `REGISTER` without `UNDO`                  |
|  [06]   | Repeated calls collapsing into one step  | One across consecutive calls of a group         | `UNDO_GROUPED`, no `UNDO`, `bl_undo_group` |
|  [07]   | Deferred callback write                  | One per write answering its step name           | `deferred` write answering `Some(name)`    |
|  [08]   | Background run                           | None until a script's `ed.undo_push()`          | Rollback scope                             |

Undo steps act on held state:
- Undo re-reads every ID its step touched at its old address, held references to touched IDs raising `ReferenceError` while untouched IDs keep working references
- `session_uid` survives undo, and `Handle.resolve` reaches the restored ID
- Undo restores registered groups, ID properties, and compositor inputs and runs no `update` callback
- Each undo push marks the file unsaved
- Window manager, screen, preference, module, and external file state stay as they are across undo
- Code reverts its own edits through its rollback scope, a `Snapshot` restore, or the inverse write
- `window_manager.undo_stack` lists `steps`, `active_index`, and `active`, reading `None` before the first push

## [07]-[CALLBACKS]

Handler, timer, msgbus, and modal bodies return `BlenderResult` to the row holding them, in a context Blender chose:
- `PathHandler`, `FrameHandler`, and `UpdateHandler` own handler arity, Blender passing one argument to a plain function of one positional parameter and padding every other callable with `None`
- `PathHandler` lifts the startup file's `""` to `Nothing`
- Depsgraph `*_post` handlers read the scene and depsgraph they receive
- `frame_set` fires `frame_change_pre` and `frame_change_post` alone
- `frame_change_pre` writes data for the frame, and a `FrameHandler` on `frame_change_post` reads evaluated results
- Timer bodies return `BlenderResult[Option[float]]`, and `Nothing` or a fault stops the timer
- Raising native timers unregister permanently, and a raising handler prints its traceback while later handlers run
- Handlers remove only themselves from their list during their call
- State seeds from a one-shot persistent `Timer` beside a `load_post` `PathHandler`, native collections included
- GPU calls outside a GPU context raise `SystemError`
- Background runs draw (`GPUOffScreen`, `batch_for_shader`, `draw_view3d`) after `gpu.init()`, and `gpu.texture.from_image` before it crashes Blender
- Background runs read `Region.data` `None`, and `RegionView3D.update()` crashes them
- `bpy.msgbus` keys name a declaring class and member (`(bpy.types.Object, "location")`) or one instance's member through `data.path_resolve("<member>", False)`
- Stock RNA writes notify msgbus subscribers coalesced after operators and outside undo
- `PERSISTENT` subscriptions survive ID remapping, never a file load
- Python writes to registered `bpy.props` publish no msgbus notification whether or not they declare `update`, and `written` publishes them
- Handler, msgbus, and update callback bodies hand ID writes to `Reports.deferred`, each write returning its undo step name as `Option[str]`
- `bpy.ops` calls release the GIL and run any live Python thread inside the operator, and threads (`multiprocessing.Queue` feeders included) join before the next `bpy` call

## [08]-[BOUNDARIES]

Each entry point translates the result once into the host's form:

| [INDEX] | [ENTRY]                         | [HOST_FORM]                                                                   |
| :-----: | :------------------------------ | :---------------------------------------------------------------------------- |
|  [01]   | `Operator` body `Ok(Done)`      | Notes at their levels and skipped faults as warnings with `{'FINISHED'}`      |
|  [02]   | `Operator` body `Ok(Unchanged)` | Reason as `INFO` with `{'CANCELLED'}`                                         |
|  [03]   | `Operator` body `Error`         | Report per rendered fault with `{'CANCELLED'}`, the file as the body found it |
|  [04]   | `poll`                          | `False` with `poll_message_set` of the rendered `BlenderUnmetFault` faults    |
|  [05]   | `draw`, draw handler            | Failed part drawn through `ui.layout.rows.alerted`, the rest drawn            |
|  [06]   | Update callback, handler, timer | Faults held by `Reports.reported` for the extension                           |
|  [07]   | `register`                      | One `RuntimeError` of rendered faults after the stack unwinds                 |
|  [08]   | `RenderEngine.render`           | `report({'ERROR'}, ...)` cancelling the render                                |
|  [09]   | Extension command line          | `CliCommand` returning 0 or 1                                                 |
|  [10]   | `--python` script               | `sys.exit` of `None`, or of the rendered faults joined by newlines for exit 1 |
|  [11]   | `execute_blender_code` body     | Raise of the rendered faults, data in `result`                                |

Faults and exceptions stay apart:
- Host refusals (`RuntimeError`, `TypeError`) become faults through `BlenderEditFault.attempted(<request>, lambda: <call>)` in place of `Ok(<call>)`
- Host calls answering `None` (`links.new` across trees) or an invalid result (`NodeLink.is_valid` false) become faults at the call
- `CANCELLED` keeps the writes a body made before its fault outside undo, and dependent steps roll back through the body's `ExitStack` scope
- Python raises inside `RenderEngine.render` complete the render as `FINISHED`, and `error_set` cancels nothing
- `modifiers.move` past a pinned modifier keeps the order with no raise or report, and code reads the order after the call
- Report entries sit in `Done.notes`, and helpers below a body return theirs in their result
- Independent per-item faults accumulate, and a body returns them in `Done.skipped` when it changed other items or as its error when it changed none

Values cross the boundary in host units and axes:
- Lengths are Blender units scaled by `unit_settings.scale_length` and angles radians, and domain values hold SI quantities
- Display strings come from `bpy.utils.units.to_string(system, category, value, split_unit=True)` and parse through `bpy.utils.units.to_value`
- `to_value` reads `-` and `+` as arithmetic and unitless numbers as the base unit
- Importers and exporters convert axes through `bpy_extras.io_utils.axis_conversion(from_forward=, from_up=, to_forward=, to_up=).to_4x4()`
- Stock exporters take `forward_axis` and `up_axis` (OBJ, STL, PLY), `axis_forward` and `axis_up` (FBX), or `export_yup` (glTF)

Operator bodies combine independent absences with `paired` before the dependent step:

```python
def opened(call: Call[Empty]) -> BlenderResult[Done | Unchanged]:
    return paired(call.root(bpy.types.Window), call.root(bpy.types.Area)).bind(lambda held: shown(*held)).map(lambda _: Done())


show = Operator[Empty](key="show_preferences", title="Preferences", description="Open the extension's preferences", undo=False, form=Empty, run=opened)
```

`shown` stands for the dependent step, a `(Window, Area) -> BlenderResult[None]` function.
