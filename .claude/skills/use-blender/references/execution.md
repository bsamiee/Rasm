# [EXECUTION]

Calls into a running Blender, the context their operators take, and the faults scripts return.

## [01]-[UNDO_STEPS]

Live calls close with an undo step named `Agent (<mode>)`:
- Consecutive calls in one mode share the step until the user acts, one Ctrl-Z reverts it
- Raises close the step with the edits before the raise inside it, a `check_is_finished` closes it after its last pass
- Reads after a user action add one empty step
- Edit Mode steps follow a hidden `MemFile Internal (pre)` substep holding the call's data-API changes to other objects, one Ctrl-Z reverts both
- `window_manager.undo_stack.steps` lists each step's `name` and `is_substep`, `undo_stack.active` is the current step

## [02]-[CHANGES]

Changes to the user's scene run as one call:
1. Convert the user's dimensions to meters at 1/64 inch
2. Write through the data API, giving each operator the context the table names
3. Read each operator's return set and the report lines on `stdout` in the same call
4. Return the evaluated values that show the change in `result`

| [INDEX] | [OPERATORS]                                  | [FORM]                                                                                |
| :-----: | :------------------------------------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `modifier_apply`, modifier operators         | `temp_override(object=o)`                                                             |
|  [02]   | `join`, `transform_apply`                    | `temp_override(active_object=o, selected_objects=[o], selected_editable_objects=[o])` |
|  [03]   | `mode_set`, `localview`, primitives, exports | `view_layer.objects.active = o` and `o.select_set(True)`, restored after              |
|  [04]   | View operators, code reading `context.area`  | `temp_override(window=v.window, area=v.area, region=v.region)`, `v = viewport()`      |

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

Use configuration.md for unit conversion.

## [03]-[PASSES]

Deferred calls run steps one event-loop pass apart:

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
- Raises inside a step answer with the step's traceback, a `finally` around the yields running and a `yield None` inside it still waiting one pass
- Past 300 s the answer is `Blender connection timed out`, the steps stop with the dropped check, and their `finally` runs at the next `gc.collect()`
- `bpy.app.timers.register(functools.partial(next, <generator>, None))` runs steps after the answer, one per `yield <seconds>`
- `persistent=True` keeps a timer across a file load, a raise ends it with the traceback on Blender's own stderr and no report

## [04]-[FAULTS]

Scripts return their record or a `Faults` record that `as_result` writes as `{"kind": "Faults", "items": [<Fault>, ...]}`:
- Each `Fault` holds `source`, the type or operator that refused, `value`, the refused input, and `accepted`, the alternatives the scene or enum holds
- Independent inputs of one call fault together in argument order, and a step that reads an earlier one runs once that one succeeds
- Input faults precede operations. Conversion and `Sheet` faults can follow scene changes or file writes
- `Object` faults name an object the scene lacks, the next call taking a name from `accepted`
- `SpaceView3D` faults name an object the largest 3D Viewport hides, local view left or the object unhidden before the next call
- `View` faults name a view outside `accepted`, or `user` with no 3D Viewport, the next call taking an axis or iso view
- For `Capture` faults (no drawable or projected extent), choose `accepted` objects or read `snapshot` bounds
- `Camera` faults name a scene camera a sheet refuses, `accepted` listing orthographic cameras
- `Path` faults name a missing `since` baseline with `accepted` listing held ones, a source sharing its stem with `accepted`, or `typst` off `PATH`
- `NodeTree` faults name a tree owner outside `accepted`, and `Window` with `None` a background `arrange`, which runs live instead
- `Operator` faults name an id outside `accepted`, or a source no importer resolves with the importers that read it
- `<category>.<operator>` faults name an option key outside `accepted`, or a source the operator refused, `convert.log` holding its message
- `wm.read_homefile` faults refuse a live `convert`, which runs in a session
- `GreasePencil` faults name an object holding no Grease Pencil, and `GreasePencilDrawing` objects holding no stroke
- `Sheet` faults name the SVG `sheet` wrote and `typst` failed to compile, the diagnostics on `stderr`

Use sessions.md for headless outcomes and refusals.
