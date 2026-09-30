# [RENDERING]

Renders of a saved file run through `headless.py render` in a background Blender under the user's preferences, renders of the live scene through the `blender` server's render tools.

## [01]-[SETTINGS]

Existing files and the factory scene `run` saves on a missing path take the declared render in constraint order, while `start` on a missing path saves the user's startup file holding it:

```python
# [HEADLESS_CALL] Cycles on the GPU, H.264 video beside the file, and the AgX view, each value after the value that resets it
import bpy

scene = bpy.context.scene
settings = scene.render
settings.engine, scene.cycles.device = "CYCLES", "GPU"
settings.image_settings.media_type = "VIDEO"
settings.ffmpeg.format, settings.ffmpeg.codec, settings.ffmpeg.constant_rate_factor = "MPEG4", "H264", "HIGH"
settings.filepath = "//renders/{blend_name}/"
scene.display_settings.display_device = "sRGB"
scene.view_settings.view_transform = "AgX"
scene.view_settings.look = "None"
bpy.ops.wm.save_mainfile()
result = {"output": settings.frame_path(frame=scene.frame_start)}
```

- `tools/interface/blender/script/startup.py` declares the startup render from the shared values in `tools/interface/render.py`
- `media_type` (`IMAGE`, `MULTI_LAYER_IMAGE`, `VIDEO`) sets `file_format` to PNG at 8 bits, `OPEN_EXR_MULTILAYER` at 16 bits, or `FFMPEG`
- `file_format` accepts formats of the current media type alone
- `VIDEO` applies Blender's H.264 preset (MKV, CRF `MEDIUM`, GOP 18) over stored ffmpeg values while `ffmpeg.video_bitrate` is 0
- `display_device` precedes `view_transform`, a display lacking the current view resets it to `Standard` with no error
- AgX looks read `AgX - <name>` (`AgX - Punchy`)
- `//` resolves against the saved file's folder, in the untitled GUI against `.artifacts/blender/` with `{blend_name}` reading `Unsaved`
- `filepaths.render_output_directory` seeds `render.filepath` of scenes created after it, existing scenes keep theirs

Use look-development.md for the site sky and sun.

## [02]-[RENDER]

Stills, frame ranges, and videos of a saved file run as one Bash call with `run_in_background: true`, its notification holding one JSON outcome:

```bash
# Current frame of <file>, one frame, a range, or `all` of the scene range, into .artifacts/blender/renders/<stem>/
python .claude/skills/use-blender/scripts/headless.py render <file> --frames <start>..<end>
```

- `sheet` names `<stem>.jpg`, up to 12 evenly spaced frames labeled by number within Read's 2000 px and 500 KB limits
- `output` names the frame pattern (`<stem>_####.png`) or the movie (`<stem>_<start>-<end>.mp4`) in the file's engine, format, size, and view
- EXR frames get a display-encoded `<frame>.jpg` beside each, the sheet's source
- `devices` names the Cycles devices of the preference compute type with `use` on, the CPU for a CPU scene or when no GPU row has `use` on
- `resumed` counts image frames an earlier run of the unchanged file wrote, which Blender skips
- Stopped Bash tasks end their Blender with whole frames on disk
- File changes (any byte, a resave after a reload included) clear the folder and render every frame, movies render whole every run
- `<stem>.log` beside the frames holds Blender's output, `rg "Saved:" <log>` lists each written file after the process clock
- `Failed` names the log, its `Error:` line stating the cause (`Cannot render, no camera` under a scene camera of `None`)
- Timings read the second render of a new scene configuration, the first compiling specialized Metal kernels
- Full-size frames read through `magick` crops within Read's limits

## [03]-[LOOK]

Look questions take one EEVEE preview for framing, then one Cycles render at the declared exposure under the site sky for the answer:

```python
# [HEADLESS_CALL] EEVEE preview of the camera at half size as <dir>/<name>-eevee.jpg
import bpy

settings = bpy.context.scene.render
settings.engine, settings.resolution_percentage, settings.image_settings.file_format = "BLENDER_EEVEE", 50, "JPEG"
settings.filepath = "<dir>/<name>-eevee.jpg"
bpy.ops.render.render(write_still=True)
result = {"preview": settings.filepath}
```

```bash
# Mean 8-bit color of a <w>x<h> region at <x>,<y> of a frame
magick <frame> -crop <w>x<h>+<x>+<y> -depth 8 -format "%[fx:round(255*mean.r)],%[fx:round(255*mean.g)],%[fx:round(255*mean.b)]" info:
```

- Previews run through `run`, which leaves the file unchanged
- Sunlit ground of albedo 0.2 reads near (122, 125, 130) and a sunlit wall of albedo 0.8 near (173, 176, 179) under the declared light and view
- EEVEE matches Cycles in sunlight and draws faces in shadow darker, shadow questions take the Cycles render
- Workbench renders take the scene exposure and read near black at -5.3, geometry reads through `capture.py`
- Rhino folds the exposure into its sun and sky, its render EXR equaling Blender's times `2 ** -5.3` and its added lights rendering 39 times brighter
- Rhino's integrator keys map one to one onto `scene.cycles` (samples, adaptive threshold, bounces, clamps, `blur_glossy`, caustics, light tree, seed)

## [04]-[PASSES]

Passes reach a file through a scene output of `MULTI_LAYER_IMAGE`, every enabled pass in each frame, or through a compositor effect whose File Output node writes chosen passes on every render:

```python
# [HEADLESS_CALL] Effect keeping the beauty and writing Image, Depth, and Normal to //passes/<stem>_####.exr
import bpy

scene, layer = bpy.context.scene, bpy.context.view_layer
layer.use_pass_z = layer.use_pass_normal = True
tree = bpy.data.node_groups.new("<tree>", "CompositorNodeTree")
tree.interface.new_socket("Image", in_out="OUTPUT", socket_type="NodeSocketColor")
scene.compositor_effects.new("<tree>").node_group = tree
layers = tree.nodes.new("CompositorNodeRLayers")
tree.links.new(layers.outputs["Image"], tree.nodes.new("NodeGroupOutput").inputs[0])
out = tree.nodes.new("CompositorNodeOutputFile")
out.format.media_type = "MULTI_LAYER_IMAGE"
out.directory, out.file_name = "//passes/", "{blend_name}_"
for kind, name in (("RGBA", "Image"), ("FLOAT", "Depth"), ("VECTOR", "Normal")):
    out.file_output_items.new(kind, name)
    tree.links.new(layers.outputs[name], out.inputs[name])
bpy.ops.wm.save_mainfile()
result = {"effects": [effect.name for effect in scene.compositor_effects]}
```

- Startup files enable the passes `Pass` in `tools/interface/render.py` lists, and Render Layers shows one output per enabled pass
- Effects with `enable_for_render` on run in stack order, and each group output's Color socket replaces the render result
- File Output nodes write on every render with the frame number after `file_name`, `mute = True` skips the write
- File Output parts take each item's name at 32-bit float, scene output parts `<view layer>.<pass>` at 16-bit half float
- Blender's bundled OpenImageIO reads every part:

```python
# [HEADLESS_CALL] Parts of a multilayer EXR with their channels
import OpenImageIO

image = OpenImageIO.ImageInput.open("<file>.exr")
parts = []
while image.seek_subimage(len(parts), 0):
    parts.append((image.spec().getattribute("name"), list(image.spec().channelnames)))
result = {"parts": parts}
```

## [05]-[LIVE]

Renders of the user's live scene serve orientation through `render_thumbnail_to_path` or `render_viewport_to_path` at the scene's own settings:

```python
# [EXECUTE_BLENDER_CODE] Close the render view a render tool opened, the area returning to its editor
import bpy

window, area = next(
    (window, area)
    for window in bpy.context.window_manager.windows
    for area in window.screen.areas
    if area.type == "IMAGE_EDITOR" and area.spaces.active.image and area.spaces.active.image.type == "RENDER_RESULT"
)
with bpy.context.temp_override(window=window, area=area):
    status = bpy.ops.render.view_cancel()
result = {"status": sorted(status), "screen": window.screen.name}
```

- Both tools write `<bpy.app.tempdir>/blender_mcp/<basename>` whatever folder the path names, answer that path, and Blender purges it at quit
- First thumbnails of a GUI process render at the scene's own size and samples, later ones at thumbnail size
- Both tools open the render view `view.render_display_type` names, and the snippet closes it
- `tools/interface/blender/script/preferences.py` declares `SCREEN`, which maximizes the area into an Image Editor
