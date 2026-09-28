# [RENDERING]

Renders run in a render process that `headless.py render` starts under the user's preferences on the saved file.

## [01]-[SETTINGS]

- Cycles GPU renders need `scene.cycles.device = "GPU"` and the add-on preference `compute_device_type` with each device's `use` flag on
- Background processes list no Cycles device until `refresh_devices()`, sessions and `render` refresh the list, and `run` renders on the CPU
- First Cycles Metal frame on a cold kernel cache compiles for over a minute, later processes reuse it
- `file_format` accepts the set `media_type`'s formats alone, video takes `render.ffmpeg.format = "MPEG4"` and `codec = "H264"`
- Renders read `scene.camera` alone, a scene camera of `None` fails with `Cannot render, no camera`
- Settings change in a live call, then the file saves, the render reads them from the file
- `//` in `render.filepath` resolves against the saved file's folder, and against `/` in an unsaved GUI session, where the write fails
- `preferences.filepaths.render_output_directory` applies to new scenes alone, an existing scene keeps its own `render.filepath`
- `render.ppm_factor` over `render.ppm_base` sets the pixel density the file records, 300 over 0.0254 writes 300 dpi
- Under a physical sky and sun lamp, exposure -5.3 puts a white ground at scene-linear 1.0, an HDRI world then renders 39 times darker
- Indirect clamps scale with exposure, the factory 10 becomes 394 at -5.3
- Material Preview applies view transform and look without exposure, Rendered under the scene world or lights applies exposure with them
- Solid mode draws through the display's `Standard` view whatever the scene transform, a Workbench render takes the scene transform and exposure
- `Khronos PBR Neutral` exists on the `sRGB` display alone, and look names carry the view prefix under AgX (`AgX - Punchy`)
- `bpy.data.colorspace.working_space` is read-only, `bpy.ops.wm.set_working_color_space` changes it

## [02]-[COMMAND]

```bash
# Frames of a file into .artifacts/blender/render/<stem>/<stem>_####, a video into one movie file
uv run --script <skill>/scripts/headless.py render <file> --frames <start>..<end>
```

- `--frames` takes one frame, `<start>..<end>`, or `all` for the scene range, the current frame without it
- Frames land in the file's engine, format, and resolution, `Rendered.files` lists each path `render.frame_path` names
- `resumed` counts image frames an earlier run of the unchanged file wrote, a changed file renders every frame again
- `<stem>.log` beside the frames holds Blender's whole output, its `Fra:` lines show progress during the render
- `magick compare <a> <b> <diff>` marks the pixels a change moved between renders under one view transform

## [03]-[COMPOSITOR]

Compositor groups need an `Image` output socket on their interface before `NodeGroupOutput` writes the render result:

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
```

- `view_layer.use_pass_z` adds the `Depth` output, `use_pass_normal` adds `Normal`, Cycles-only passes sit on `view_layer.cycles`
- File Output nodes write on every render, `mute = True` on the node skips the write
- Renders pass through the group while `scene.render.use_compositing` is true
- Multilayer EXRs hold one part per layer, Blender's bundled `OpenImageIO` reads every part
