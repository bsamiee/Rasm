# [SEQUENCER]

## [01]-[SCENES]

Strips sit on `context.sequencer_scene`, the workspace's `sequencer_scene`, apart from window scene `context.scene`:
- Background runs hold no sequencer scene until `context.workspace.sequencer_scene = scene`
- `scene.sequence_editor_create()` returns the editor, `strips` holding top-level strips and `strips_all` every strip, meta children included
- Strip names are unique across `strips_all`, a name taken in any meta taking `.001`
- Keys and drivers address `sequence_editor.strips_all["<name>"]`
- `sequence_editor.channels[n]` is channel `n` from 1 to 128, `mute` hiding its strips from renders and `lock` leaving Python writes open

Renders take strips from the scene they render:
- `bpy.ops.render.render(write_still=True, use_sequencer_scene=True)` renders the sequencer scene at its own frame, size, and `render.filepath`
- `use_sequencer_scene=True` raises `RuntimeError` with no sequencer scene set
- `render.use_sequencer` (on) renders strips while an unmuted non-sound strip exists, else the 3D scene, and frames with no strip render transparent
- Strips blend in `scene.sequencer_colorspace_settings` (`sRGB`), movie and image strips decoding through their own `colorspace_settings`
- Sequencer output passes the scene view transform, AgX turning pure red orange, and edits output for display set `view_transform` to `Standard`
- Video renders hold no audio at the `render.ffmpeg.audio_codec` default `NONE`, and `bpy.ops.sound.mixdown` writes audio alone

## [02]-[STRIPS]

Strips come from `strips.new_*` on the editor or a meta, file-add operators failing `poll` outside a Sequencer area:
- Calls place a selected strip at `channel` and `frame_start`, an occupied span staying overlapped
- `new_movie` makes no sound strip, a movie's audio entering through `new_sound` over the same file
- Missing media makes a one-frame strip with no raise, a movie reading `fps` 0.0
- Image sequences grow one frame per `strip.elements.append(<file name>)`, names resolving against `strip.directory`
- Scene strips take the source scene's range at creation, later source range writes moving `content_duration` alone
- `new_effect` takes `length` for effects with no input (`COLOR`, `TEXT`), `COLOR` strips taking render resolution as writable `width` and `height`
- `TEXT` strips place `location` as a frame fraction from (0.5, 0.5) and size `font_size` in full-size frame pixels scaled by `resolution_percentage`

Movie strips hold one timeline frame per media frame whatever the scene rate, sound strips the media's duration (a 4-second 30 fps file in a 24 fps scene makes a 120-frame movie and a 96-frame sound). Retiming conforms a movie to scene `rate = render.fps / render.fps_base`. `retiming_keys.add(timeline_frame=strip.left_handle)` creates start and end keys, and an end key at `timeline_frame = content_start + round(content_duration * rate / strip.fps)` plays every media frame over the sound's span.

Cut lists place each source by in frame and length in scene frames, end to end or overlapping by `blend` on alternating channels. `placed` adds each strip with content `first` frames before its slot, then trims its left handle to the slot before the next strip exists and meets no overlap:

```python
type Clip = tuple[Path, int, int]


def cut(add: Callable[[str, str, int, int], bpy.types.Strip], clips: Sequence[Clip], /, *, start: int = 1, channel: int = 1, blend: int = 0) -> tuple[bpy.types.Strip, ...]:
    def placed(index: int, clip: Clip, at: int) -> bpy.types.Strip:
        path, first, length = clip
        strip = add(path.stem, str(path), channel + (index % 2 if blend else 0), at - first)
        strip.left_handle, strip.duration = at, length
        return strip

    return tuple(map(placed, count(), clips, accumulate((length - blend for *_, length in clips), initial=start)))
```

`cut(editor.strips.new_movie, clips, blend=12)` and `cut(editor.strips.new_sound, clips, channel=4, blend=12)` place picture and sound on one timing. Each `pairwise` picture pair takes `new_effect(name=..., type="CROSS", channel=3, frame_start=incoming.left_handle, input1=outgoing, input2=incoming)`, a 12-frame cross. An `add` conforming each strip `new_movie` returns retimes every movie at placement, before its handle writes and while every earlier strip holds keys.

## [03]-[TIMING]

Writes to strip timing members move content and handles:

| [INDEX] | [MEMBER]             | [WRITE]                                                    |
| :-----: | :------------------- | :--------------------------------------------------------- |
|  [01]   | `left_handle`        | Moves visible start, content staying placed                |
|  [02]   | `right_handle`       | Moves visible end, leaving out the frame at it             |
|  [03]   | `duration`           | Sets `right_handle` from `left_handle`                     |
|  [04]   | `content_start`      | Slides content, both handles, and keys on strip members    |
|  [05]   | `content_trim_start` | Cuts source frames, `content_start` and `left_handle` move |
|  [06]   | `content_trim_end`   | Cuts source frames from the end, `right_handle` moving     |
|  [07]   | `channel`            | Shuffles off an occupied span in the written direction     |

Timing writes clamp to content and resolve overlaps:
- Handles past content hold the boundary frame, durations clamp to 1, and a handle written across the other sits one frame from it
- Single images and `COLOR` strips move content with `left_handle`, image sequences keeping it placed
- Handle, `duration`, and `content_start` writes leaving the strip over another push it up a channel
- Renames rewrite key paths on strip members
- `strip.split(frame, "SOFT")` keeps full content in both halves and returns the right half, `"HARD"` cutting content
- `split` at the left handle or outside the strip returns `None`

Retiming keys sit on movie, image, scene, and sound strips:
- `retiming_keys.add(timeline_frame=...)` inside the handles adds the key with start and end keys
- `add` returns `None` outside the handles (default 0 included) and on missing media
- `timeline_frame` writes crash Blender while a strip earlier in `strips` holds no retiming keys, a `COLOR` strip or a sound included
- End key writes stretch content and handles, start key writes and writes past a neighboring key change nothing
- `retiming_keys.reset()` removes every key

## [04]-[EFFECTS_AND_METAS]

Effect strips take their timing from their inputs:
- `new_effect` orders inputs by start, `input_1` first, and keeps the channel given
- Effects render only from a channel above both inputs, read-only handles spanning the inputs' overlap and butted inputs making a zero-length effect
- Removing an input strip removes every effect reading it
- `CROSS` mixes by `(frame - left_handle) / duration`, and `use_default_fade = False` holds the mix at `effect_fader`
- Overlapping strips composite higher channels over lower ones by `blend_type` (`ALPHA_OVER`), `blend_alpha` setting opacity

`strip.modifiers.new(name=..., type=...)` returns the class its type names (`ColorBalanceModifier`), and modifiers run before the strip transform. `COMPOSITOR` modifiers and `COMPOSITOR` effect strips run `node_group` over scene-linear values in any sequencer space. Modifiers read their own strip, effects read input strips at Color inputs in order, the fade at the first Float input, and `properties.inputs` values at later inputs.

Metas group strips through their own `strips` and channels:
- `new_meta` makes a one-frame meta, and `strip.move_to_meta(meta)` moves a strip, an effect input bringing its effect and other input
- Metas fit their children through `left_handle_offset = 0`, then `right_handle_offset = 0`, and meta `content_start` writes move children in time
- Child channels count inside their meta, and meta trims hide children outside the meta's handles
- `strip.parent_meta()` returns the holding meta, and `meta.separate()` moves children to top level at their timeline positions
