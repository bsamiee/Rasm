# [ANIMATION]

Keyframes sit in layered actions: an action holds slots, one per animated ID, and each slot's F-curves sit in a channelbag of the action's keyframe strip.

## [01]-[KEYS]

```python
# [EXECUTE_BLENDER_CODE] Location keys, linear interpolation, and the evaluated position between them
import bpy
from bpy_extras import anim_utils

scene = bpy.context.scene
obj = bpy.data.objects["<Object>"]
frame = scene.frame_current
obj.location = (0.0, 0.0, 0.0)
obj.keyframe_insert("location", frame=1)
obj.location = (4.0, 0.0, 0.0)
obj.keyframe_insert("location", frame=24)
animation = obj.animation_data
channelbag = anim_utils.action_get_channelbag_for_slot(animation.action, animation.action_slot)
for fcurve in channelbag.fcurves:
    for key in fcurve.keyframe_points:
        key.interpolation = "LINEAR"
scene.frame_set(12)
x_at_12 = obj.matrix_world.translation.x
scene.frame_set(frame)
result = {"action": animation.action.name, "slot": animation.action_slot.identifier, "fcurves": [(f.data_path, f.array_index, len(f.keyframe_points)) for f in channelbag.fcurves], "x_at_12": x_at_12}
```

- First `keyframe_insert` on an ID creates the action `<Object>Action` with slot `OB<Object>`, one layer, and one `KEYFRAME` strip
- `channelbag.fcurves.find("location", index=0)` returns one F-curve or `None`
- New keys take `preferences.edit.keyframe_new_interpolation_type`, `BEZIER` here, a linear motion sets each key

## [02]-[SHARED_ACTIONS]

- Assigning an existing action to another ID leaves `animation_data.action_slot` at `None`, and the ID stays still
- `animation_data.action_suitable_slots` lists slots the ID can take, assigning one to `action_slot` binds it
- `action.slots.new(id_type="OBJECT", name="<name>")` gives a second ID motion of its own inside one action
- `keyframe_insert` on an ID with an action and no slot adds a slot for that ID to the shared action

## [03]-[OUTPUT]

- Viewport playblasts (`render.opengl(animation=True)`) need the GUI, render processes render through an engine
