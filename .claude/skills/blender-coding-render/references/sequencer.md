# [SEQUENCER]

## [01]-[SCENES]

Strips sit on `context.sequencer_scene` (workspace `sequencer_scene`), while `context.scene` names the window scene:
- Background runs hold no sequencer scene until `context.workspace.sequencer_scene = scene`
- `bpy.ops.render.render(write_still=True, use_sequencer_scene=True)` renders the sequencer scene at its own frame, size, and `render.filepath`
- Renders asking for a sequencer scene with none set, or with no unmuted strip other than sound, raise `RuntimeError`
- `render.use_sequencer` renders strips while one unmuted strip other than sound exists, else the 3D scene, empty frames rendering transparent black
- Sequencer renders apply the scene's view transform over strip colors
- `scene.sequencer_colorspace_settings` (`sRGB`) names the space strips blend in, movie and image strips taking `colorspace_settings` from their files
- `scene.sequence_editor_create()` returns the editor, `strips` holding its top-level strips and `strips_all` every strip depth first
- `sequence_editor.channels[n]` is channel `n`, its `mute` hiding strips from renders and its `lock` leaving Python writes open

## [02]-[STRIPS]

Strips come from the editor's `strips.new_*` calls, file-add operators failing `poll` outside a Sequencer editor area:
- Calls place a strip on the span given, an occupied span staying overlapped
- Created strips start selected, and operators reading `selected_strips` act on each of them
- Missing media files make a one-frame strip with no raise, a movie reading `fps` 0.0
- Image sequences grow one frame per `strip.elements.append(file)`, file names relative to `strip.directory`
- Movie strips hold one timeline frame per media frame whatever `render.fps`, and sound strips convert by duration
- Scene strips take the source scene's frame range at creation, later source range changes moving `content_duration` alone
- Scene strips showing their own sequencer scene render transparent
- `new_effect` takes `length` for effects with no input (`COLOR`, `TEXT`), and two-input effects raise `RuntimeError` without both inputs
- `COLOR` strips take render resolution at creation as writable `width` and `height`, the space `transform` scales in

## [03]-[TIMING]

Writes to strip timing members move content and handles:

| [INDEX] | [MEMBER]                                 | [WRITE]                                        |
| :-----: | :--------------------------------------- | :--------------------------------------------- |
|  [01]   | `left_handle`                            | Moves visible start, content staying placed    |
|  [02]   | `right_handle`                           | Moves visible end, the first frame left out    |
|  [03]   | `duration`                               | Sets `right_handle` from `left_handle`         |
|  [04]   | `content_start`                          | Slides content and both handles together       |
|  [05]   | `content_trim_start`, `content_trim_end` | Cuts content, moving its start or end          |
|  [06]   | `channel`                                | Shuffles off an occupied span in its direction |

Timing writes clamp to content and resolve overlaps:
- Handles past content hold the boundary frame, and durations clamp to 1
- Handles written across the other handle sit one frame from it
- Single images and `COLOR` strips move content with `left_handle`, image sequences keep it placed
- Handle, `duration`, and `content_start` writes that overlap another strip push the written strip up a channel
- `strip.split(frame, "SOFT")` keeps full content in both halves and returns the right half (`<name>.001`), and `"HARD"` cuts content
- Splits at a left handle or outside the strip return `None`
- `retiming_keys.add(timeline_frame=...)` adds a key with start and end keys, and moving the last key stretches content and handles
- `retiming_keys.reset()` undoes retiming, and retiming key writes on a strip with missing media crash Blender

## [04]-[EFFECTS_AND_METAS]

Strips combine through effects, modifiers, blending, and metas:
- `new_effect` orders inputs to start `input_1` first and keeps the channel given
- Effects render only from a channel above both inputs, and their handles are read-only, spanning input overlap or the gap between inputs
- Overlapping strips composite the higher channel over lower ones by its `blend_type`, `ALPHA_OVER` by default, `blend_alpha` setting its opacity
- `CROSS` mixes by `(frame - left_handle) / duration`, and `use_default_fade = False` holds the mix at `effect_fader`
- `strip.modifiers.new(name=..., type=...)` adds modifiers (`COLOR_BALANCE`, `CURVES`, `COMPOSITOR`, `MASK`) that run before the strip transform
- `new_meta` makes a one-frame meta, and `strip.move_to_meta(meta)` moves a strip, an effect input bringing its effect and other input
- Metas fit their children through `left_handle_offset = 0`, then `right_handle_offset = 0`, and meta `content_start` writes move children in time
- Child channels count inside their meta, and meta trims hide children outside its handles
- `strip.parent_meta()` returns the meta holding a strip, and `meta.separate()` returns children to top level at their timeline positions

`COMPOSITOR` modifiers and effect strips run a group over scene-linear values in any sequencer space, a modifier on its own strip. Effect strips take input strips at Color inputs in order, the fade at the first Float input, and `properties.inputs` values at later inputs:

```python
def composited(editor: bpy.types.SequenceEditor, strip: bpy.types.Strip, group: bpy.types.CompositorNodeTree) -> Result[bpy.types.CompositorStrip, RuntimeError]:
    def grouped(made: bpy.types.Strip) -> bpy.types.CompositorStrip:
        effect: bpy.types.CompositorStrip = made  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        effect.node_group = group
        return effect

    return catch(editor.strips.new_effect, exception=RuntimeError)(name=group.name, type="COMPOSITOR", channel=strip.channel + 1, frame_start=strip.left_handle, input1=strip).map(grouped)
```
