# [EXECUTION]

Code runs in the user's GUI through `blender` `execute_blender_code` and in background Blender through `headless.py`, each call wrapped by `scripts/wrapper.py`.

## [01]-[CALLS]

`.claude/settings.json` runs `wrapper.py` as the PreToolUse hook of both servers' `execute_blender_code`, and `headless.py` wraps `run` and `call` code the same way:
- Calls run in a fresh namespace on Blender's main thread between event-loop passes, the window and other clients wait until a call returns
- Code compiles as `<agent>`, tracebacks and warnings count its lines as sent
- Each warning prints once per line on `stderr` as `<agent>:<line>: <Category>: <message>`

Live calls close with one `Agent (<mode>)` undo step:
- Consecutive calls in one mode share the step until the user acts, one Ctrl-Z reverts it
- Raises close the step with the edits before the raise inside it, a `check_is_finished` closes it after its last pass
- Every call marks the file modified and deletes the user's redo steps, a read after a user action adds one empty step
- Edit Mode steps follow a hidden `MemFile Internal (pre)` substep holding the call's data-API changes to other objects, one Ctrl-Z reverts both
- `window_manager.undo_stack.steps` lists each step's `name` and `is_substep`, `undo_stack.active` is the current step

## [02]-[CHANGES]

Changes to the user's scene run as one call:
1. Convert the user's dimensions to meters, `bpy.utils.units.to_value("IMPERIAL", "LENGTH", "10' 6\"")`, exact by `round(m / 0.0254 * 64) / 64 * 0.0254`
2. Write through the data API, which reads no context, and call operators for work the data API holds no member for
3. Give each operator the context the table names
4. Read each operator's return set and the report lines on `stdout` in the same call
5. Return the evaluated values that show the change in `result`

| [INDEX] | [OPERATORS]                                  | [FORM]                                                                                 |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `modifier_apply`, `join`, `transform_apply`  | `temp_override(active_object=o, selected_objects=[o], selected_editable_objects=[o])`  |
|  [02]   | `mode_set`, `localview`, primitives, exports | `view_layer.objects.active = o` and `o.select_set(True)`, restored after               |
|  [03]   | View operators, code reading `context.area`  | `temp_override(window=v.window, area=v.area, region=v.region)`, `v = scene.viewport()` |

- Calls hold the first window in context with no area or region, and a background call the file's first stored window
- Areas alone fail a view operator's poll, and areas of another window without `window=` raise `TypeError: Area not found in screen`
- Override keywords naming no context member do nothing
- `"<name>" in dir(bpy.ops.<category>)` reads whether an operator is registered

```python
# [EXECUTE_BLENDER_CODE] Context-reading operator on a named object through a selection override
import bpy

target = bpy.data.objects["<Object>"]
with bpy.context.temp_override(active_object=target, selected_objects=[target], selected_editable_objects=[target]):
    status = bpy.ops.object.shade_auto_smooth()
result = {"status": sorted(status)}
```

Each operator reading decides the next step:

| [INDEX] | [READING]                              | [MEANING]                                         | [NEXT]                           |
| :-----: | :------------------------------------- | :------------------------------------------------ | :------------------------------- |
|  [01]   | `{'FINISHED'}`                         | Ran, no-op included (`transform_apply` flags off) | Read the changed data            |
|  [02]   | `{'CANCELLED'}` with no line           | No target (`modifier_apply` of no modifier)       | Check names and override members |
|  [03]   | `Info:` or `Warning:` line on `stdout` | Report beside either return set                   | Act on the message               |
|  [04]   | `RuntimeError: Error: <message>`       | Error report, its line also on `stdout`           | Fix the cause the message names  |
|  [05]   | `poll() failed, context is incorrect`  | Context member missing                            | Override form of the table       |

## [03]-[PASSES]

Work reading state an event-loop pass applies (workspace switches, area and region sizes, view matrices, a later pass's evaluation) runs as one deferred call:

```python
# [EXECUTE_BLENDER_CODE] Steps one event-loop pass apart, answered with the dict the last step yields
import functools

import bpy


def steps():
    obj = bpy.data.objects["<Object>"]
    obj.location.x = <x>
    yield None
    yield {"x": obj.matrix_world.translation.x}


check_is_finished = functools.partial(next, steps(), None)
```

- `blender` calls `check_is_finished` between passes and answers with the first dict it returns, each `yield None` waits one pass
- Answers hold `stdout` and `stderr` printed before the first pass, later prints stay out of them
- Raises inside a step answer with the step's traceback
- Past 300 s the answer is `Blender connection timed out` and the steps stop with the dropped check
- `bpy.app.timers.register(functools.partial(next, <generator>, None))` runs steps after the answer, one per `yield <seconds>`
- `persistent=True` keeps a timer across a file load, a raise ends it with the traceback on Blender's own stderr and no report

## [04]-[READS]

Reads return values in `result` and change nothing:
- `plain(<struct>)` from `rna.py` returns an RNA struct's stored values as JSON
- `obj.evaluated_get(bpy.context.evaluated_depsgraph_get())` holds counts and dimensions after modifiers
- `bpy.utils.units.to_string("IMPERIAL", "LENGTH", m, split_unit=True)` reports `10' 6"`
- `divmod(round(m / 0.0254 * 64) / 64, 12)` gives feet and inches to 1/64"
- Reports state ft² and ft³ as meters over `0.3048` per dimension, lb as kg over `0.45359237`
- Camera `lens` and `sensor_width` take the `CAMERA` unit, millimeters under every system

## [05]-[ANSWERS]

Each part of a `blender` answer decides the next call:

| [INDEX] | [PART]                         | [HOLDS]                                                  | [NEXT]                                 |
| :-----: | :----------------------------- | :------------------------------------------------------- | :------------------------------------- |
|  [01]   | `result`                       | Values, `repr` of a value JSON cannot hold               | Convert the value in code              |
|  [02]   | `stdout`                       | Prints and operator report lines                         | Operator readings                      |
|  [03]   | `stderr`                       | `<agent>:<line>: DeprecationWarning`, handler tracebacks | Replace the deprecated member          |
|  [04]   | `message` of status `error`    | Traceback, `stdout` before the raise beside it           | Read the state, continue from the line |
|  [05]   | `Blender connection timed out` | Blender still working on the call                        | Read what landed in the next call      |

- Tracebacks end in the `File "<agent>", line <n>` frame of the sent line that raised, frames above it belong to the bridge and `wrapper.py`

Use sessions.md for routes, background context, and headless outcomes.
