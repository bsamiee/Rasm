# [RENDERING]

Renders through `headless.py render` in a process under the user's preferences on the saved file.

## [01]-[SETTINGS]

- Engine identifiers are `BLENDER_EEVEE`, `CYCLES`, and `BLENDER_WORKBENCH`
- Cycles GPU renders need `scene.cycles.device = "GPU"` and the add-on preference `compute_device_type` with each device's `use` flag on
- Factory processes list no Cycles device until `refresh_devices()` and render a GPU scene on the CPU
- Processes under the user's preferences read the stored device rows, sessions and `render` refresh them
- First Cycles Metal frame on a cold kernel cache compiles past the `blender` server's wait, later processes reuse the cache
- `image_settings.media_type` writes before `file_format`, and `file_format` accepts that media type's formats alone
- Video takes `media_type = "VIDEO"`, `render.ffmpeg.format = "MPEG4"`, and `codec = "H264"`
- Sequencer strips sit in `scene.sequence_editor.strips`
- Renders read `scene.camera` alone, a scene camera of `None` fails with `Cannot render, no camera`
- `scene.eevee.time_limit` caps an EEVEE render in seconds, 0 renders every sample
- `//` in `render.filepath` resolves against the saved file's folder, a render of an unsaved file to a `//` path fails
- `filepaths.render_output_directory` applies to `bpy.data.scenes.new()` alone, existing scenes and `scene.new(type="EMPTY")` keep `render.filepath`
- `render.ppm_factor` over `render.ppm_base` sets the pixel density the file records, 300 over 0.0254 writes 300 dpi
- Site scenes stay physical under AgX, and exposure -5.3 puts a white ground under the physical sky's 53.5° sun at scene-linear 1.0
- Rhino folds that exposure into its lights, a Rhino render EXR compares to a Blender EXR times `2 ** exposure`
- Exposure -5.3 renders every other world 39 times darker than the physical sky
- `cycles.sample_clamp_indirect` keeps the factory 10 under any exposure, `10 / 2 ** exposure` (394 at -5.3) keeps bounce light
- Material Preview applies view transform and look without exposure, Rendered under the scene world or lights applies exposure with them
- Solid mode draws through the display's `Standard` view whatever the scene transform, a Workbench render takes the scene transform and exposure
- `display_settings.display_device` writes before `view_transform`, a display lacking the current view resets it to `Standard` with no error
- `Khronos PBR Neutral` exists on the `sRGB` display alone, and look names hold the view prefix under AgX (`AgX - Punchy`)
- `bpy.data.colorspace.working_space` is read-only, `bpy.ops.wm.set_working_color_space` changes it

## [02]-[COMMAND]

```bash
# Frames of a file into .artifacts/blender/renders/<stem>/<stem>_####, a video into one movie file
python .claude/skills/use-blender/scripts/headless.py render <file> --frames <start>..<end>
```

- `--frames` takes one frame, `<start>..<end>`, or `all` for the scene range, the current frame without it
- Frames render in the file's engine, format, and resolution, `Rendered.files` lists each path `render.frame_path` names
- `render_thumbnail_to_path` renders 320 px on the long side at 16 samples, a picture for orientation alone
- Look questions take one render each
- `resumed` counts image frames an earlier run of the unchanged file wrote, a changed file renders every frame again
- `<stem>.log` beside the frames holds Blender's whole output, one `render | Saved:` line per written frame

## [03]-[COMPOSITOR]

Compositing is a stack of effects on `scene.compositor_effects`, each holding a `CompositorNodeTree`:

```python
# [EXECUTE_BLENDER_CODE] Beauty to the result, Image, Depth, and Normal to one multilayer EXR under <dir>
import bpy

scene, layer = bpy.context.scene, bpy.context.view_layer
layer.use_pass_z = layer.use_pass_normal = True
tree = bpy.data.node_groups.new("<tree>", "CompositorNodeTree")
tree.interface.new_socket("Image", in_out="OUTPUT", socket_type="NodeSocketColor")
scene.compositor_effects.new("<tree>").node_group = tree
layers = tree.nodes.new("CompositorNodeRLayers")
tree.links.new(layers.outputs["Image"], tree.nodes.new("NodeGroupOutput").inputs[0])
out = tree.nodes.new("CompositorNodeOutputFile")
out.directory, out.file_name = "<dir>/", "passes"
for kind, name in (("RGBA", "Image"), ("FLOAT", "Depth"), ("VECTOR", "Normal")):
    out.file_output_items.new(kind, name)
    tree.links.new(layers.outputs[name], out.inputs[name])
```

- `NodeGroupOutput` writes the render result from the tree interface's first output socket, a Color socket alone
- First interface input sockets take the Combined pass in the first effect and the previous effect's first output after it
- `view_layer.use_pass_z` adds the `Depth` output, `use_pass_normal` adds `Normal`, Cycles-only passes sit on `view_layer.cycles`
- File Output nodes write on every render, `mute = True` on the node skips the write
- Renders pass through each effect with `enable_for_render` on in stack order while `scene.render.use_compositing` is true
- Renders skip every effect while `render.use_sequencer` is on and an unmuted top-level strip other than sound exists
- `enable_for_preview` gates an effect in the viewport
- `bpy.ops.scene.new_compositor_effect_node_group()` adds an effect with an `Image` input and output
- Properties tab `COMPOSITOR` shows the effect stack, `show_properties_compositor` hides it per area
- Multilayer EXRs hold one part per layer, Blender's bundled `OpenImageIO` reads every part
