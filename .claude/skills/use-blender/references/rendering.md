# [RENDERING]

Renders run in a render process that `headless.py render` starts under the user's preferences on the saved file.

## [01]-[SETTINGS]

- Proof renders set their own `scene.cycles.samples` over the startup scene's 1024
- Cycles GPU renders need `scene.cycles.device = "GPU"` and the add-on preference `compute_device_type` with each device's `use` flag on
- Background processes list no Cycles device until `refresh_devices()`, sessions and `render` refresh the list, and `run` renders on the CPU
- First Cycles Metal frame on a cold kernel cache compiles for over a minute, later processes reuse it
- `image_settings.media_type` takes `IMAGE`, `MULTI_LAYER_IMAGE`, or `VIDEO` before `file_format`, which accepts the set media type's formats alone
- Video takes `render.ffmpeg.format = "MPEG4"` and `render.ffmpeg.codec = "H264"` after `media_type = "VIDEO"`
- Renders read `scene.camera` alone, a scene camera of `None` fails with `Cannot render, no camera`
- Settings change in a live call, then the file saves, the render reads them from the file
- `//` in `render.filepath` resolves against the saved file's folder, and against `/` in an unsaved GUI session, where the write fails
- `preferences.filepaths.render_output_directory` applies to new scenes alone, an existing scene keeps its own `render.filepath`
- `render.ppm_factor` over `render.ppm_base` sets the pixel density the file records, 300 over 0.0254 writes 300 dpi
- Exposure -5.3 puts a white ground under the physical sky and sun lamp at scene-linear 1.0, an HDRI world renders 39 times darker
- Indirect clamps scale with exposure, the startup's 394 is the factory 10 at -5.3
- Material Preview applies view transform and look without exposure, Rendered under the scene world or lights applies exposure with them
- Solid mode draws through the display's `Standard` view whatever the scene transform, a Workbench render takes the scene transform and exposure
- `Khronos PBR Neutral` exists on the `sRGB` display alone, and look names carry the view prefix under AgX (`AgX - Punchy`)
- `bpy.data.colorspace.working_space` is read-only, `bpy.ops.wm.set_working_color_space` changes it

## [02]-[COMMAND]

```bash
# Frames of a file into .artifacts/blender/<stem>/<stem>_####, a video into one movie file
uv run --script <skill>/scripts/headless.py render <file> --frames <start>..<end>
```

- `--frames` takes one frame, `<start>..<end>`, or `all` for the scene range, the current frame without it
- Frames land in the file's engine, format, and resolution, each with its `path` from `render.frame_path` and the `mean` and `deviation` of its pixels
- Videos count as written up to the movie's `frame_duration`, a frame past it is missing
- `Rendered` holds every frame on disk with pixels varying beyond one 8-bit level
- `resumed` counts image frames an earlier run of the unchanged file wrote, a changed file renders every frame again
- `Incomplete` lists missing and blank frames of a run that exited 0 and deletes the blank ones for the next run
- `<stem>.log` beside the frames holds Blender's whole output, its `Fra:` lines show progress during the render
- `magick compare <a> <b> <diff>` marks the pixels a change moved between renders under one view transform

## [03]-[COMPOSITOR]

`scene.compositing_node_group` holds a `CompositorNodeTree` whose `NodeGroupOutput` writes the render result, the group interface needs an `Image` output socket first:

```python
# [EXECUTE_BLENDER_CODE] Beauty to the result, Image, Depth, and Normal to one multilayer EXR under <dir>
import bpy

scene, layer = bpy.context.scene, bpy.context.view_layer
layer.use_pass_z = layer.use_pass_normal = True
tree = bpy.data.node_groups.new("<tree>", "CompositorNodeTree")
tree.interface.new_socket("Image", in_out="OUTPUT", socket_type="NodeSocketColor")
scene.compositing_node_group = tree
layers = tree.nodes.new("CompositorNodeRLayers")
tree.links.new(layers.outputs["Image"], tree.nodes.new("NodeGroupOutput").inputs[0])
out = tree.nodes.new("CompositorNodeOutputFile")
out.directory, out.file_name = "<dir>/", "passes"
out.format.media_type = "MULTI_LAYER_IMAGE"
for kind, name in (("RGBA", "Image"), ("FLOAT", "Depth"), ("VECTOR", "Normal")):
    out.file_output_items.new(kind, name)
    tree.links.new(layers.outputs[name], out.inputs[name])
result = {"linked": [i.name for i in out.inputs if i.is_linked], "valid": all(link.is_valid for link in tree.links)}
```

- `view_layer.use_pass_z` adds the `Depth` output, `use_pass_normal` adds `Normal`, Cycles-only passes sit on `view_layer.cycles`
- File Output nodes write on every render, proofs included, `mute = True` on the node skips the write
- Renders pass through the group while `scene.render.use_compositing` is true
- Multilayer EXRs hold one part per layer, Blender's bundled `OpenImageIO` reads every part
