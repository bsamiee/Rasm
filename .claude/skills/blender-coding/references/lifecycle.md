# [LIFECYCLE]

- Use `use-blender` for background run behavior

## [01]-[FILE_LOADS]

File loads drop every handler without `persistent`, every timer registered without `persistent=True`, every msgbus subscription, and every `driver_namespace` entry, and `register()` never runs for the new file. Per-file state rebuilds in one persistent `load_post` handler:
- Startup runs `load_post` once after extensions register, every callback `register()` acquired live
- Extensions enabled within a session see no `load_post`, and a one-shot persistent timer beside the handler seeds their state
- `load_post` runs after versioning and before drivers and the depsgraph evaluate, evaluated reads and heavy work running from later timer steps
- Subscriptions renew in the handler through `msgbus.clear_by_owner(<owner>)`, then every key subscribed again
- `load_post_fail` and `save_post_fail` close the state `load_pre` and `save_pre` opened, and `save_pre` writes reach the save that fired it
- GUI loads through `wm.open_mainfile` or `wm.read_homefile` leave `context.window` `None`, later context-reading calls running under `temp_override(window=window_manager.windows[0])`

## [02]-[UPDATES]

`depsgraph_update_post` handlers receive `(scene, depsgraph)` and read `depsgraph.updates` during the call:
- Each update's `id` is the evaluated ID with `session_uid` 0, and bodies copy `id.original.session_uid` and the `is_updated_*` flags before the call returns
- `depsgraph.id_type_updated(<id_type>)` tests a whole ID type before a walk of `updates`
- Handlers fire after `view_layer.update()` and `evaluated_depsgraph_get()`, background runs included
- Writes inside the handler evaluate within that update and fire no further call, and `view_layer.update()` inside it calls the handler again before returning
- Viewport transforms publish no msgbus notification and reach depsgraph handlers alone
- Any animation recalculation sets `is_updated_shading`
- F12 renders fire the handler with empty `updates`, and undo fires one update listing the scene and every object
- Built-in property writes from a timer notify msgbus subscribers before that tick's `depsgraph_update_post`

## [03]-[DEFERRED_WORK]

Handler, msgbus, update callback, and render-thread bodies hand ID writes to one main-thread timer step that waits for an unlocked interface and a ready predicate, then pushes an undo step named for a write that changed data. Background runs hold no event loop and write at the call with no undo step:

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


@bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
def moved(_scene: bpy.types.Scene, depsgraph: bpy.types.Depsgraph, /) -> None:
    def grounded(uids: frozenset[int], /) -> bool:
        sunk = tuple(obj for obj in bpy.data.objects if obj.session_uid in uids and obj.location.z < 0.0)
        for obj in sunk:
            obj.location.z = 0.0
        return bool(sunk)

    if uids := frozenset(
        original.session_uid for update in depsgraph.updates if update.is_updated_transform and isinstance(update.id, bpy.types.Object) and isinstance(original := update.id.original, bpy.types.Object)
    ):
        scheduled(lambda: True, grounded, uids)
```

Writes end their own update cycle:
- Update callbacks fire on every assignment, equal values included, and writes assign only differing values
- Comparisons against a read-back value run at stored precision (`FloatProperty` as `float32`, theme colors as bytes, `maxlen` in UTF-8 bytes)
- Update callbacks skip `property_unset`, and derived state is computed where read
- Timers run beside a GUI render, `is_interface_locked` reading true while the render holds `use_lock_interface`

Work outlasting one callback runs as a process on `sys.executable`, Blender's bundled interpreter without `bpy`, and its result applies as a write once the process exits:

```python
def spawned(argv: tuple[str, ...], applied: Write[[subprocess.Popen[bytes]]], /) -> None:
    worker = subprocess.Popen((sys.executable, *argv))
    scheduled(lambda: worker.poll() is not None, applied, worker)
```

`applied` reads the result file its arguments name and `worker.wait()`, which returns at once from a timer step and blocks a background run until the process exits. `multiprocessing` and `concurrent.futures` process pools re-run Blender's `__main__` from its `__file__` (`<blender string>` or the `--python` script) in each child, which fails before work starts.

## [04]-[FRAMES]

Frame and `render_*` handlers run on the render job thread beside viewport drawing during a GUI render, and on the calling thread under `render.render` in exec form:
- Handlers changing data the viewport reads run under `RenderSettings.use_lock_interface`
- Job-wide writes and per-job state go in `render_init`, which runs once before the render reads the scene
- Restores run from both `render_complete` and `render_cancel`
- Timers registered from the job thread run on the main thread at the next event-loop pass, and `scheduled` writes leave the job thread through them

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

`index` names an array channel and stays `-1` for a scalar, an array driven at `-1` returning a list of every channel's F-curve. Drivers evaluate by host rules:
- `variables.new()` returns a `SINGLE_PROP` variable named `var` with an `OBJECT` target, and an `id` of a type other than `id_type` raises `TypeError`
- `driver.is_simple_expression` answers whether Blender evaluates the expression without Python
- Expressions over variables, `math` names, and `driver_namespace` functions evaluate with auto-run off, and attribute reads and other names stay invalid until the file's scripts are trusted
- Code writes each ID property a driver reads, then `id.update_tag()` on that ID, before the driver's first evaluation
- Drivers that first evaluated without their property recover through `source.update_tag()`, `driver.is_valid = True`, and `owner.update_tag()` on the driven ID, then one evaluation
- `driver.is_valid = True` alone sets the flag with the value left stale, and tags without that write leave the driver invalid
