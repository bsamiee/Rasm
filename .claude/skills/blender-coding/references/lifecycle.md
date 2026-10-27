# [LIFECYCLE]

- Use `use-blender` for background run behavior

## [01]-[FILE_LOADS]

File loads drop callbacks:
- Handlers without `persistent`
- Timers registered without `persistent=True`
- Every msgbus subscription
- Every `driver_namespace` entry

`register()` never runs again for the new file, and per-file state rebuilds from a persistent `load_post` handler:
- Startup runs `load_post` once after extensions register, every callback `register()` acquired live
- Extensions enabled within a session see no `load_post`, and a one-shot persistent timer beside the handler seeds their state
- `load_post` runs after versioning and before drivers and the depsgraph evaluate, evaluated reads waiting for a later step
- Heavy `load_post` work queues one item per timer call
- Subscriptions renew in one `load_post` handler that calls `msgbus.clear_by_owner(<owner>)` and subscribes every key again
- `load_post_fail` and `save_post_fail` close the state `load_pre` and `save_pre` opened
- `save_pre` writes reach the save that fired it
- GUI code calling `wm.open_mainfile` or `wm.read_homefile` reads `context.window` `None` after the load
- Context-reading calls after a GUI load run under `temp_override(window=window_manager.windows[0])`

## [02]-[UPDATES]

`depsgraph_update_post` handlers receive `(scene, depsgraph)` and read `depsgraph.updates` during the call:
- Bodies copy each update's `id.original.session_uid` and `is_updated_*` flags before keying later work, native updates expiring with the call
- `depsgraph.id_type_updated("<ID type>")` (`"IMAGE"`) tests a whole type before a walk of `updates`
- `depsgraph_update_post` fires after `view_layer.update()` and `evaluated_depsgraph_get()`, background runs included
- Viewport transforms publish no msgbus notification and reach depsgraph handlers alone
- Any animation recalculation sets `is_updated_shading`
- F12 renders fire the handler with empty `updates`, and undo fires one update listing the scene and every object
- Writes made inside `depsgraph_update_post` evaluate within that update and fire no further handler call
- `view_layer.update()` inside `depsgraph_update_post` calls the handler again before it returns
- Built-in property writes from a timer notify msgbus subscribers before that tick's `depsgraph_update_post`

## [03]-[DEFERRED_WRITES]

Handler, msgbus, and update callback bodies defer ID writes to one `bpy.app.timers` function per argument set, and background runs write at the call with no undo step:

```python
def marked(uids: frozenset[int]) -> bool:
    changed = tuple(block for block in bpy.data.objects if block.session_uid in uids and not block.get("moved"))
    for block in changed:
        block["moved"] = True
    return bool(changed)


def deferred(uids: frozenset[int]) -> float | None:
    manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    if manager.is_interface_locked:
        return 0.0
    if marked(uids):
        bpy.ops.ed.undo_push(message=bpy.path.display_name(marked.__name__))
    return None


@bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
def moved(_scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph) -> None:
    uids = frozenset(
        original.session_uid
        for update in depsgraph.updates
        if update.is_updated_transform and isinstance(update.id, bpy.types.Object) and isinstance(original := update.id.original, bpy.types.Object)
    )
    if uids and bpy.app.background:
        marked(uids)
    elif uids:
        bpy.app.timers.register(partial(deferred, uids))
```

Deferred writes follow host scheduling:
- Native timers run beside a GUI render with no interface-lock test
- Timers test `bpy.app.is_job_running(<job>)` per job type (`RENDER`, `RENDER_PREVIEW`, `OBJECT_BAKE`, `COMPOSITE`, `SHADER_COMPILATION`)
- Timers registered from the render job thread run on the main thread at the next event-loop pass
- Update callbacks fire on every assignment, equal values included, and writes assign only differing values to end the cycle
- Comparisons against a read-back value run at stored precision (`FloatProperty` as `float32`, theme colors as bytes, `maxlen` in UTF-8 bytes)
- Update callbacks skip `property_unset`, and derived state is computed where read

## [04]-[FRAMES]

Frame and `render_*` handlers run on the render job thread beside viewport drawing during a GUI render:
- `render_init` runs once before the scene is read and `render_pre` after settings copy
- Job-wide writes and per-job state go in `render_init`
- Restores run from both `render_complete` and `render_cancel`
- `render.render` in exec form runs its handlers on the calling thread
- Before a render with handlers that change data the viewport reads, set `RenderSettings.use_lock_interface`

## [05]-[DRIVERS]

Drivers survive auto-run off when built from variables and a simple expression, each target writing `id_type` from its source before `id`:

```python
def driven(owner: bpy.types.ID, member: str, index: int, source: bpy.types.ID, source_path: str) -> bpy.types.Driver:
    driver: bpy.types.Driver = owner.driver_add(member, index).driver  # type: ignore[union-attr, assignment]  # ty: ignore[unresolved-attribute, invalid-assignment]
    variable = driver.variables.new()
    variable.type = "SINGLE_PROP"
    target = variable.targets[0]
    target.id_type, target.id, target.data_path = source.id_type, source, source_path
    driver.expression = variable.name
    return driver
```

Drivers evaluate by host rules:
- `driver.is_simple_expression` answers whether an expression evaluates with auto-run off, and an `id` of a type other than `id_type` raises `TypeError`
- Expressions reading `bpy.context` or data paths outside variables stay invalid until the file's scripts are trusted
- Code writes each ID property a driver reads, then `id.update_tag()` on that ID, before the driver's first evaluation
- Drivers that first evaluated without their property recover through `source.update_tag()`, `driver.is_valid = True`, and `owner.update_tag()` on the driven ID, then one evaluation
- `driver.is_valid = True` alone sets the flag with the value left stale, and tags without that write leave the driver invalid
- `driver_namespace` functions run with auto-run off
- Code building a driver with an expression that is not simple reads `preferences.filepaths.use_scripts_auto_execute` first

## [06]-[WORKERS]

Work outlasting one callback runs outside Blender's Python threads:
- Long work runs in a subprocess, polled from a timer function that reads its progress and applies results on the main thread
- Entry points in a background run wait for the subprocess at their call
- Worker queues drain on each tick until empty with the worker finished, a one-shot drain stranding items queued later
