# [RENDERING]

Renders run in a render process that `headless.py render` starts under the user's preferences on the saved file.

## [01]-[SETTINGS]

- Startup scenes hold 4096 adaptive Cycles samples, a proof sets its own `scene.cycles.samples`
- First Cycles Metal frame on a cold kernel cache compiles for over a minute, later processes reuse it
- `image_settings.media_type` takes `IMAGE`, `MULTI_LAYER_IMAGE`, or `VIDEO` before `file_format`, which accepts the set media type's formats alone
- Video takes `render.ffmpeg.format = "MPEG4"` and `render.ffmpeg.codec = "H264"` after `media_type = "VIDEO"`
- Renders read `scene.camera` alone, a scene camera of `None` fails with `Cannot render, no camera`
- Settings change in a live call, then the file saves, the render reads them from the file

## [02]-[COMMAND]

```bash
# Frames of a file into .artifacts/blender/<stem>/<stem>_####, a video into one movie file
uv run --script <root>/.claude/skills/use-blender/scripts/headless.py render <file> --frames <start>..<end>
```

- `--frames` takes one frame, `<start>..<end>`, or `all` for the scene range, the current frame without it
- Frames land in the file's engine, format, and resolution, each with its `path` from `render.frame_path` and the `mean` and `deviation` of its pixels
- Videos count as written up to the movie's `frame_duration`, a frame past it is missing
- `<stem>.log` beside the frames holds Blender's whole output, its `Fra:` lines show progress during the render

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

## [04]-[CHECKS]

- `magick compare <a> <b> <diff>` marks the pixels a change moved between renders under one view transform
