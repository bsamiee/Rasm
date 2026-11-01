# [LIFECYCLE]

- Use `use-blender` for background run behavior

## [01]-[FILE_LOADS]

File loads drop non-`persistent` handlers and timers, msgbus subscriptions, and `driver_namespace` entries, and `register()` skips the new file. Per-file state rebuilds in one persistent `load_post` handler:
- Startup runs `load_post` once after extensions register, every callback `register()` acquired live
- Extensions enabled within a session see no `load_post`, and a one-shot persistent timer beside the handler seeds their state
- `load_post` runs after versioning and before drivers and the depsgraph evaluate, evaluated reads and heavy work running from later timer steps
- Subscriptions renew in the handler through `msgbus.clear_by_owner(<owner>)`, then every key subscribed again
- `load_post_fail` and `save_post_fail` close the state `load_pre` and `save_pre` opened, and `save_pre` writes reach the save that fired it
- GUI `wm.open_mainfile` or `wm.read_homefile` loads leave `context.window` `None` for the calling step
- Later context reads in a GUI load's step run under `temp_override(window=window_manager.windows[0])`

## [02]-[UPDATES]

`depsgraph_update_post` handlers receive `(scene, depsgraph)` and read `depsgraph.updates` during the call:
- Each update `id` is an evaluated ID with `session_uid` 0, and bodies copy `id.original.session_uid` and `is_updated_*` flags before returning
- `depsgraph.id_type_updated(<id_type>)` tests a whole ID type before a walk of `updates`
- Handlers fire after `view_layer.update()` and `evaluated_depsgraph_get()`, background runs included
- Handler writes evaluate within that update and fire no further call, but `view_layer.update()` inside it reruns the handler before returning
- Transform operators, script calls included, publish no msgbus notification and reach depsgraph handlers alone
- Any animation recalculation sets `is_updated_shading`
- F12 renders can fire the handler with empty `updates`, and undo fires one listing the scene, every object, and each other ID the step re-read
- Timer writes to built-in properties notify msgbus subscribers as the step returns, after any `depsgraph_update_post` its `ed.undo_push` fired

## [03]-[DEFERRED_WORK]

Handler, msgbus, update, and render-thread bodies hand ID writes to a main-thread timer step waiting for an unlocked interface and a ready predicate. Steps push an undo step named for a write that changed data, unpushed timer writes leaving `is_dirty` false and dropping at the next undo. Background runs hold no event loop and write at the call with no undo step:

```python
class Write[**P](Protocol):
    __name__: str

    def __call__(self, *args: P.args, **kwargs: P.kwargs) -> bool: ...


def scheduled[**P](ready: Callable[[], bool], write: Write[P], /, *args: P.args, **kwargs: P.kwargs) -> None:
    def step() -> float | None:
        manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        if manager.is_interface_locked or not ready():
            return 0.1
        if write(*args, **kwargs):
            bpy.ops.ed.undo_push(message=bpy.path.display_name(write.__name__))
        return None

    if bpy.app.background:
        write(*args, **kwargs)
    else:
        bpy.app.timers.register(step)
```

`depsgraph_update_post` handlers copy each transformed original's `session_uid` and call `scheduled(lambda: True, grounded, uids)`. The step pushes a `Grounded` undo step when `grounded` lifts a sunk object to `z = 0` and returns `True`.

Writes end their own update cycle:
- Update callbacks fire on every assignment, equal values included, and writes assign only differing values
- Comparisons against a read-back value run at stored precision (`FloatProperty` as `float32`, theme colors as bytes, `maxlen` in UTF-8 bytes)
- Update callbacks skip `property_unset`, and derived state is computed where read
- Timers run through GUI renders, `is_interface_locked` true for the whole job under `use_lock_interface`
- Timer writes during a locked render reach `depsgraph_update_post` after `render_complete`

Work outlasting one callback runs as a `sys.executable` process (bundled Python without `bpy`), its result applied as a write once the process exits:

```python
def spawned(argv: tuple[str, ...], applied: Write[[subprocess.Popen[bytes]]], /) -> None:
    worker = subprocess.Popen((sys.executable, *argv))
    scheduled(lambda: worker.poll() is not None, applied, worker)
```

`applied` reads its result file and calls `worker.wait()`, returning at once in a timer step and blocking a background run until exit. Process pools (`multiprocessing`, `concurrent.futures`) fail before work starts, each child re-running `__main__` from `__file__` (`<blender string>`, a `--python` script).

## [04]-[FRAMES]

Frame and `render_*` handlers run on the render job thread beside viewport drawing in a GUI render, on the calling thread under `EXEC_DEFAULT` `render.render`:
- Handlers changing data the viewport reads run under `RenderSettings.use_lock_interface`
- `render_init` runs once before the render reads the scene and holds job-wide writes and per-job state
- Restores run from both `render_complete` and `render_cancel`
- Timers registered from the job thread run on the main thread at the next event-loop pass while the render continues
- `scheduled` writes leave the job thread through a timer registered from the job thread

## [05]-[DRIVERS]

Drivers read sources through variables, each target writing `id_type` from its source before `id`:

```python
def driven(owner: bpy.types.ID, path: str, source: bpy.types.ID, source_path: str, /, *, index: int = -1) -> bpy.types.Driver:
    driver: bpy.types.Driver = owner.driver_add(path, index).driver  # type: ignore[union-attr, assignment]  # ty: ignore[unresolved-attribute, invalid-assignment]
    variable = driver.variables.new()
    target = variable.targets[0]
    target.id_type, target.id, target.data_path = source.id_type, source, source_path
    driver.expression = variable.name
    return driver
```

`index` names an array channel and stays `-1` for a scalar, an array driven at `-1` returning a list of every channel's F-curve.
- `variables.new()` returns a `SINGLE_PROP` variable named `var` with an `OBJECT` target, and an `id` not matching `id_type` raises `TypeError`
- `driver_add(path, index)` returns the existing driver F-curve on a second call
- Python-created driver F-curves hold keys at (0, 0) and (1, 1) that remap the driver value as they move
- `driver.is_simple_expression` reports whether Blender evaluates the expression without Python
- Expressions over variables, `math` names, and `driver_namespace` functions evaluate with auto-run off
- Attribute reads and other names in an expression stay invalid until a file's scripts are trusted
- Code writes each ID property a driver reads, then `id.update_tag()` on that ID, before the driver's first evaluation
- Drivers first evaluated without their property need `source.update_tag()`, `driver.is_valid = True`, driven `owner.update_tag()`, then evaluation
- `driver.is_valid = True` alone sets the flag with the value left stale, and tags without that write leave the driver invalid
