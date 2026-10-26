# [LIFECYCLE]

Covers callback bodies across file loads, updates, deferred writes, frames, drivers, and worker processes.

- Use `use-blender` for background run behavior

## [01]-[FILE_LOADS]

File loads drop callbacks:
- Handlers without `persistent`
- Timers registered without `persistent=True`
- Every msgbus subscription
- Every `driver_namespace` entry

`register()` never runs again for the new file, and per-file state rebuilds from a `load_post` `PathHandler` body:
- Startup runs `load_post` once after extensions register, every callback `register()` acquired live
- `load_post` runs after versioning and before drivers and the depsgraph evaluate, evaluated reads waiting for a later step
- Heavy `load_post` work queues one item per `Timer` tick
- `Subscription` rows hold keys on extension `BUS`, and one `load_post` renewal clears `BUS` and subscribes every key again
- `Attachment.seed` writes its record onto hosts the loaded file left unstored
- `load_post_fail` and `save_post_fail` close the state `load_pre` and `save_pre` opened
- `save_pre` writes reach the save that fired it
- GUI code calling `wm.open_mainfile` or `wm.read_homefile` reads `context.window` `None` after the load
- Context-reading calls after a GUI load run under `temp_override(window=window_manager.windows[0])`

## [02]-[UPDATES]

`UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=..., body=..., changes=...)` delivers updates of its ID types during the handler call:
- Bodies copy each update through `Update.of` (original `Handle` and change flags) before keying later work, native updates expiring with the call
- `changes=Some(Change.GEOMETRY | Change.SHADING)` filters by the update's flags
- `depsgraph.id_type_updated("<ID type>")` (`"IMAGE"`) tests a whole type before a walk of `updates`
- `depsgraph_update_post` fires after `view_layer.update()` and `evaluated_depsgraph_get()`, background runs included
- Viewport transforms publish no msgbus notification and reach depsgraph handlers alone
- Any animation recalculation sets `is_updated_shading`
- F12 renders fire the handler with empty `updates`, and undo fires one update listing the scene and every object
- Writes made inside `depsgraph_update_post` evaluate within that update and fire no further handler call
- `view_layer.update()` inside `depsgraph_update_post` calls the handler again before it returns
- Built-in property writes from a timer notify msgbus subscribers before that tick's `depsgraph_update_post`

Bodies hand writes to the deferred writer of their row's scope:

```python
@contextmanager
def acquisitions(identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
    with identity.reports.deferred(refreshed) as deferred:

        def touched(_: bpy.types.Scene, updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
            for update in updates:
                deferred(Update.of(update).handle)
            return Ok(None)

        yield Ok((UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.Image,), body=touched),))
```

`refreshed` stands for a `Handle[ID] -> BlenderResult[bool]` write that assigns only values that differ and answers `True` for a change made. `deferred` names the undo step from the write function.

## [03]-[DEFERRED_WRITES]

`Reports.deferred(write)` picks its executor once when its scope opens and yields a callable over hashable handles and records:
- GUI sessions coalesce calls with equal arguments into one pending main-thread timer
- Pending timers wait while the interface is locked during a render, then run the write, hold its faults, and push its step name through `ed.undo_push`
- Background runs run the write at each call, hold its faults, and drop its step name
- Native timers run beside a GUI render with no interface-lock test
- `bpy.app.is_job_running` gates native timers per job type (`RENDER`, `RENDER_PREVIEW`, `OBJECT_BAKE`, `COMPOSITE`, `SHADER_COMPILATION`)
- Timers registered from the render job thread run on the main thread at the next event-loop pass
- Update callbacks fire on every assignment, equal values included, and writes assign only differing values to end the cycle under either executor
- Comparisons against a read-back value run at stored precision (`FloatProperty` as `float32`, theme colors as bytes, `maxlen` in UTF-8 bytes)
- Update callbacks skip `property_unset`, and derived state is computed where read
- Scope exit cancels every pending call

## [04]-[FRAMES]

Frame and `render_*` handlers run on the render job thread beside viewport drawing during a GUI render:
- `render_init` runs once before the scene is read and `render_pre` after settings copy
- Job-wide writes go in `render_init` and per-frame writes in `frame_change_pre`
- Per-job state starts in `render_init`, and writes go through `Reports.deferred`
- Restores run from both `render_complete` and `render_cancel`
- `render.render` in exec form runs its handlers on the calling thread
- Before a render with handlers that change data the viewport reads, set `RenderSettings.use_lock_interface`

## [05]-[DRIVERS]

Drivers survive auto-run off when built from variables and simple expressions:
- Expressions reading `bpy.context` or data paths outside variables stay invalid until the file's scripts are trusted
- Code writes each ID property a driver reads, then `id.update_tag()` on that ID, before the driver's first evaluation
- Drivers that first evaluated without their property recover through `source.update_tag()`, `driver.is_valid = True`, and `owner.update_tag()` on the driven ID, then one evaluation
- `driver.is_valid = True` alone sets the flag with the value left stale, and tags without that write leave the driver invalid
- `driver_namespace` functions run with auto-run off
- Persistent `load_post` bodies restore each `driver_namespace` entry after a load
- `view_layer.update()` inside a `driver_namespace` function raises `RuntimeError("Dependency graph update requested during evaluation")`
- `Animation` with a `Driven` source checks `filepaths.use_scripts_auto_execute` when its expression is not simple

## [06]-[WORKERS]

Work outlasting one callback runs outside Blender's Python threads:
- Long work runs in a subprocess, polled from a `Timer` body that reads its progress and applies results on the main thread
- Entry points in a background run wait for the subprocess at their call
- Worker queues drain on each tick until empty with the worker finished, a one-shot drain stranding items queued later
