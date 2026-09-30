# [ANIMATION]

Layered actions hold keys, one slot per animated ID, each slot's F-curves in a channelbag of the action's keyframe strip.

## [01]-[NEW_MOTION]

New motion keys the property at each frame, sets interpolation per key, and reads the evaluated result at a frame between keys:

```python
# [EXECUTE_BLENDER_CODE] <Object> moved 10' along X from frame 1 to 48 at constant speed, read at frame 24
import bpy
from bpy_extras import anim_utils

FOOT = 0.3048
scene = bpy.context.scene
obj = bpy.data.objects["<Object>"]
start = obj.location.copy()
for frame, offset in ((1, 0.0), (48, 10 * FOOT)):
    obj.location.x = start.x + offset
    obj.keyframe_insert("location", index=0, frame=frame)
obj.location = start
animation = obj.animation_data
bag = anim_utils.action_get_channelbag_for_slot(animation.action, animation.action_slot)
for fcurve in bag.fcurves:
    for key in fcurve.keyframe_points:
        key.interpolation = "LINEAR"
current = scene.frame_current
scene.frame_set(24)
x_at_24 = obj.matrix_world.translation.x / FOOT
scene.frame_set(current)
result = {
    "action": animation.action.name,
    "slot": animation.action_slot.identifier,
    "keys": [(f.data_path, f.array_index, [tuple(k.co) for k in f.keyframe_points]) for f in bag.fcurves],
    "x_at_24_ft": round(x_at_24, 4),
}
```

- First `keyframe_insert` calls create action `<Object>Action` with slot `OB<Object>`, one layer, and one `KEYFRAME` strip, and assign both
- Key values are absolute property values, offset from the rest value their object holds
- `scene.frame_set` evaluates parents, constraints, and drivers and runs frame handlers, each call returning to the user's frame after its read
- `fcurve.evaluate(<frame>)` reads one channel at a frame without moving the scene
- Baselines taken before keying show each new channel in snapshot `changed`

Bulk keys write through the channelbag, one `foreach_set` per F-curve:

```python
# [EXECUTE_BLENDER_CODE] <Object> lifted 2' and back over frames 1, 24, and 48 in its own slot of <Action>, read at frame 24
import bpy

FOOT = 0.3048
scene = bpy.context.scene
obj, action = bpy.data.objects["<Object>"], bpy.data.actions["<Action>"]
data = obj.animation_data_create()
data.action = action
slot = action.slots.new(id_type="OBJECT", name=obj.name)
data.action_slot = slot
curve = action.layers[0].strips[0].channelbag(slot, ensure=True).fcurves.new("location", index=2)
rest = obj.location.z
frames, values = (1.0, 24.0, 48.0), (rest, rest + 2 * FOOT, rest)
curve.keyframe_points.add(len(frames))
curve.keyframe_points.foreach_set("co", [v for pair in zip(frames, values, strict=True) for v in pair])
for key in curve.keyframe_points:
    key.interpolation = "LINEAR"
curve.update()
current = scene.frame_current
scene.frame_set(24)
lift = (obj.matrix_world.translation.z - rest) / FOOT
scene.frame_set(current)
result = {"slots": [s.identifier for s in action.slots], "lift_at_24_ft": round(lift, 4), "users": action.users}
```

## [02]-[SHARED_ACTIONS]

One action animates many IDs through one slot each:
- Actions assigned to another ID leave `action_slot` at `None` and that ID still, `action_suitable_slots` listing slots it can take
- Listed slots assigned to `action_slot` give an ID that slot's motion, shared with the ID it was keyed on
- `action.slots.new(id_type="OBJECT", name=<name>)` then `action_slot = <slot>` gives an ID motion of its own inside one action
- `keyframe_insert` on an ID holding an action with no slot adds and binds a slot named after that ID
- `strip.channelbag(<slot>, ensure=True)` returns the slot's channelbag, `anim_utils.action_get_channelbag_for_slot` it or `None`
- `action.frame_range` spans the keys of every slot

## [03]-[EXISTING_MOTION]

Existing motion is read from `snapshot("<name>", objects=)`, where each object's `animation` holds `Animated` (action, slot, channels with key count, interpolations, and a hash) or `Unassigned` (action, suitable slots). Changes run between that baseline and a comparison:
- `key.co_ui.x += <frames>` on each key of a curve retimes it, handles moving with each key
