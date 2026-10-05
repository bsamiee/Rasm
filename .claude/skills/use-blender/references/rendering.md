# [RENDERING]

Saved files and copies of the live file render through `headless.py render` under the user's preferences, one Cycles render under site light per look.

## [01]-[SETTINGS]

Files made from the user's startup file hold the declared render, and another file takes it in one call writing each value after the one resetting it:
1. Engine, then device
2. `media_type`, then `file_format` and the ffmpeg values of its type
3. Output path
4. `display_device`, then `view_transform`, then `look`

```python
# [HEADLESS_CALL] Cycles on the GPU, H.264 video beside the file, and the AgX view under look <look>, each value after the value that resets it
import bpy

scene = bpy.context.scene
settings = scene.render
settings.engine, scene.cycles.device = "CYCLES", "GPU"
settings.image_settings.media_type = "VIDEO"
settings.ffmpeg.format, settings.ffmpeg.codec, settings.ffmpeg.constant_rate_factor = "MPEG4", "H264", "HIGH"
settings.filepath = "//renders/{blend_name}/"
scene.display_settings.display_device = "sRGB"
scene.view_settings.view_transform = "AgX"
scene.view_settings.look = "<look>"
bpy.ops.wm.save_mainfile()
result = {"output": settings.frame_path(frame=scene.frame_start)}
```

- `tools/interface/blender/script/startup.py` declares the startup render from the shared values in `tools/interface/render.py`
- `media_type` (`IMAGE`, `MULTI_LAYER_IMAGE`, `VIDEO`) sets `file_format` to PNG at 8 bits, `OPEN_EXR_MULTILAYER` at 16 bits, or `FFMPEG`
- `file_format` accepts formats of the current media type alone
- `VIDEO` applies Blender's H.264 preset (MKV, CRF `MEDIUM`, GOP 18) over stored ffmpeg values while `ffmpeg.video_bitrate` reads 0
- Displays lacking the current view reset it to `Standard`, the display written first keeping every view and look pair it offers
- AgX looks read `AgX - <name>` (`AgX - Punchy`), `None` holding the plain view
- `//` resolves against the saved file's folder, `frame_path` naming a movie `<filepath><start>-<end>.<container>`
- `filepaths.render_output_directory` seeds `render.filepath` of scenes created after it, existing scenes keeping theirs

## [02]-[RENDER]

Stills, frame ranges, and videos of a saved file run as one Bash call with `run_in_background: true`, its notification holding one JSON outcome:

```bash
# Current frame of <file>, one frame, a range, or `all` of the scene range, into .artifacts/blender/renders/<stem>/
python .claude/skills/use-blender/scripts/headless.py render <file> --frames <start>..<end>
```

Each outcome field decides the next read:

| [INDEX] | [FIELD]        | [DECIDES]                                                                                 |
| :-----: | :------------- | :---------------------------------------------------------------------------------------- |
|  [01]   | `sheet`        | `<stem>.jpg` of up to 12 evenly spaced frames labeled by number, the first `Read`         |
|  [02]   | `output`       | Frame pattern `<stem>_####.png` or movie `<stem>_<start>-<end>.<container>` to crop       |
|  [03]   | `devices`      | Cycles devices that rendered, the CPU for a CPU scene or with no GPU row's `use` on       |
|  [04]   | `resumed`      | Image frames an earlier run of the unchanged file wrote, which Blender skips              |
|  [05]   | `camera`       | Scene camera the frames show                                                              |
|  [06]   | `Failed` `log` | Log whose `Error:` line states the cause (`Cannot render, no camera` under camera `None`) |

- Frames take the file's engine, format, size, and view, EXR frames getting a display-encoded `<frame>.jpg` beside each as the sheet's source
- File changes (any byte, a resave after a reload included) clear the folder and render every frame, and movies render whole every run
- Stopped Bash tasks end their Blender with whole frames on disk
- `<stem>.log` beside the frames holds Blender's output, `rg "Saved:" <log>` listing each written file after the process clock
- Timings read the second render of a new scene configuration, the first compiling specialized Metal kernels
- Full-size frames read through `magick` crops within Read's 2000 px and 500 KB

## [03]-[LIVE_SCENE]

Renders of the user's live scene run on a copy, the GUI's file and modified flag unchanged:
1. Save the live file's current data as `<name>.blend` through the copy call sessions.md holds
2. `headless.py render .artifacts/blender/<name>.blend --frames current` in the background renders it at the scene's own size and samples
3. `Read` the `sheet` the outcome names

## [04]-[LOOK]

Look questions take one EEVEE preview for framing, then one Cycles render at the declared exposure under the site sky for the answer:
1. Run the preview through `run`, which leaves the file unchanged
2. Render the camera through `headless.py render`
3. Read mean colors of chosen regions against the readings below

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

- Sunlit ground of albedo 0.2 reads near (122, 125, 130) and a sunlit wall of albedo 0.8 near (173, 176, 179) under the declared light and view
- EEVEE matches Cycles in sunlight and draws faces in shadow darker, shadow questions taking the Cycles render
- Workbench renders take the scene exposure and read near black at -5.3, geometry reading through `capture.py`
- Rhino folds the exposure into its sun and sky, its render EXR equaling Blender's times `2 ** -5.3` and its added lights rendering 39 times brighter
- Rhino's integrator keys map one to one onto `scene.cycles` (samples, adaptive threshold, bounces, clamps, `blur_glossy`, caustics, light tree, seed)

Use look-development.md for the site sky and sun.

## [05]-[PASSES]

Passes reach a file through a `MULTI_LAYER_IMAGE` scene output of every enabled pass, or a compositor effect writing chosen passes per render:

```python
# [HEADLESS_CALL] Effect <tree> keeping the beauty and writing Image, Depth, and Normal to //passes/<stem>_####.exr
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
- File Output nodes write on every render with the frame number after `file_name`, `mute = True` skipping the write
- File Output parts take each item's name at 32-bit float uncompressed, scene output parts `<view layer>.<pass>` at 16-bit half float
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

- Parts read `Image.R` to `.A` for RGBA, `Depth.V` for FLOAT, and `Normal.X` to `.Z` for VECTOR
